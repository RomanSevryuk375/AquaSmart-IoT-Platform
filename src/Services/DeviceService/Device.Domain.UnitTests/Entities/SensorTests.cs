namespace Device.Domain.UnitTests.Entities;

public class SensorTests
{
    [Fact]
    public void Update_WithValidData_UpdatesPropertiesAndRaisesEvent()
    {
        // Arrange
        Sensor sensor = new SensorBuilder().Build();
        var newControllerId = Guid.NewGuid();
        string newName = "Updated Sensor";
        ConnectionProtocol newProtocol = ConnectionProtocol.OneWire;
        string newAddress = "28FF4A1B2C3D4E5F";

        // Act
        Result result = sensor.Update(newControllerId, newName, newProtocol, newAddress);

        // Assert
        result.IsSuccess.Should().BeTrue();
        sensor.ControllerId.Should().Be(newControllerId);
        sensor.Name.Value.Should().Be(newName);
        sensor.ConnectionAddress.Protocol.Should().Be(newProtocol);
        sensor.ConnectionAddress.Address.Should().Be(newAddress);

        sensor.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<SensorUpdatedDomainEvent>()
            .Which.Should().Match<SensorUpdatedDomainEvent>(e =>
                e.SensorId == TestConstants.SensorId &&
                e.ControllerId == newControllerId &&
                e.Name == newName);
    }

    [Fact]
    public void Update_WithInvalidData_ReturnsFailureAndNoEvents()
    {
        // Arrange
        Sensor sensor = new SensorBuilder().Build();

        // Act
        Result result = sensor.Update(
            TestConstants.ControllerId, "New Name", ConnectionProtocol.OneWire, "0x11");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain(CommonErrors.OneWireInvalidFormat);

        sensor.Name.Value.Should().Be("Test Sensor");
        sensor.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void SetName_WithValidName_UpdatesNameWithoutEvent()
    {
        // Arrange
        Sensor sensor = new SensorBuilder().Build();
        string newName = "Just New Name";

        // Act
        Result result = sensor.Update(
            sensor.ControllerId,
            newName,
            sensor.ConnectionAddress.Protocol,
            sensor.ConnectionAddress.Address);

        // Assert
        result.IsSuccess.Should().BeTrue();
        sensor.Name.Value.Should().Be(newName);
        sensor.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<SensorUpdatedDomainEvent>()
            .Which.Name.Should().Be(newName);
    }

    [Fact]
    public void SetState_WithNewState_UpdatesStateAndRaisesEvent()
    {
        // Arrange
        Sensor sensor = new SensorBuilder().Build();
        SensorState newState = SensorState.Active;

        // Act
        sensor.SetState(newState);

        // Assert
        sensor.State.Should().Be(newState);
        sensor.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<SensorStateChangedDomainEvent>()
            .Which.State.Should().Be(newState);
    }

    [Fact]
    public void SetState_WithSameState_DoesNotRaiseEvent()
    {
        // Arrange
        Sensor sensor = new SensorBuilder().Build();
        SensorState oldState = sensor.State;

        // Act
        sensor.SetState(oldState);

        // Assert
        sensor.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void MarkAsDeleted_RaisesDeletedEvent()
    {
        // Arrange
        Sensor sensor = new SensorBuilder().Build();

        // Act
        sensor.MarkAsDeleted();

        // Assert
        sensor.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<SensorDeletedDomainEvent>()
            .Which.SensorId.Should().Be(TestConstants.SensorId);
    }
}
