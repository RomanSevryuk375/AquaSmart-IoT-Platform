// Ignore Spelling: Telemetry

#pragma warning disable IDE1006 // Naming Styles

namespace Telemetry.Domain.UnitTests.Entities;

public class SensorTests
{
    [Fact]
    public void Create_WithValidParameters_ReturnsSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var controllerId = Guid.NewGuid();
        var ecosystemId = Guid.NewGuid();
        string name = "Temperature Sensor";
        SensorType type = SensorType.Temperature;
        SensorState state = SensorState.Active;
        string unit = " C ";
        double lastValue = 24.5;
        DateTime updatedAt = DateTime.UtcNow;
        DateTime createdAt = DateTime.UtcNow;

        // Act
        Result<Sensor> result = Sensor.Create(
            id, controllerId, ecosystemId, name, type, state, unit, lastValue, updatedAt, createdAt);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(id);
        result.Value.ControllerId.Should().Be(controllerId);
        result.Value.EcosystemId.Should().Be(ecosystemId);
        result.Value.Name.Value.Should().Be("Temperature Sensor");
        result.Value.Type.Should().Be(type);
        result.Value.State.Should().Be(state);
        result.Value.Unit.Should().Be("C");
        result.Value.LastValue.Should().Be(lastValue);
        result.Value.UpdatedAt.Should().Be(updatedAt);
        result.Value.CreatedAt.Should().Be(createdAt);
        result.Value.IsDataDelayed.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null!)]
    public void Create_WithEmptyUnit_ReturnsValidationFailure(string invalidUnit)
    {
        // Arrange & Act
        Result<Sensor> result = Sensor.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "SensorName", SensorType.Temperature,
            SensorState.Active, invalidUnit, 25.0, DateTime.UtcNow, DateTime.UtcNow);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Sensor.Invalid");
        result.Error.Message.Should().Be("Unit must not be empty.");
    }

    [Fact]
    public void Create_WithInvalidName_ReturnsValidationFailure()
    {
        // Arrange & Act
        Result<Sensor> result = Sensor.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "", SensorType.Temperature,
            SensorState.Active, "C", 25.0, DateTime.UtcNow, DateTime.UtcNow);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Sensor.Invalid");
        result.Error.Message.Should().Be(CommonErrors.NameEmpty);
    }

    [Fact]
    public void Update_WithValidParameters_UpdatesPropertiesAndIncrementsVersion()
    {
        // Arrange
        Sensor sensor = new SensorBuilder().WithUnit("C").Build();
        Guid initialVersion = sensor.Version;
        var newControllerId = Guid.NewGuid();

        // Act
        Result result = sensor.Update(newControllerId, SensorType.Voltage, " V ");

        // Assert
        result.IsSuccess.Should().BeTrue();
        sensor.ControllerId.Should().Be(newControllerId);
        sensor.Type.Should().Be(SensorType.Voltage);
        sensor.Unit.Should().Be("V");
        sensor.Version.Should().NotBe(initialVersion);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null!)]
    public void Update_WithEmptyUnit_ReturnsValidationFailure(string invalidUnit)
    {
        // Arrange
        Sensor sensor = new SensorBuilder().WithUnit("C").Build();
        Guid initialVersion = sensor.Version;

        // Act
        Result result = sensor.Update(Guid.NewGuid(), SensorType.Temperature, invalidUnit);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Sensor.Invalid");
        result.Error.Message.Should().Be("Unit must not be empty.");
        sensor.Version.Should().Be(initialVersion);
    }

    [Fact]
    public void SetName_WithValidName_UpdatesNameAndIncrementsVersion()
    {
        // Arrange
        Sensor sensor = new SensorBuilder().WithName("Old Name").Build();
        Guid initialVersion = sensor.Version;

        // Act
        Result result = sensor.SetName("New Name");

        // Assert
        result.IsSuccess.Should().BeTrue();
        sensor.Name.Value.Should().Be("New Name");
        sensor.Version.Should().NotBe(initialVersion);
    }

    [Fact]
    public void SetName_WithInvalidName_ReturnsValidationFailure()
    {
        // Arrange
        Sensor sensor = new SensorBuilder().WithName("Old Name").Build();
        Guid initialVersion = sensor.Version;

        // Act
        Result result = sensor.SetName("");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DeviceName.Invalid");
        result.Error.Message.Should().Be(CommonErrors.NameEmpty);
        sensor.Version.Should().Be(initialVersion);
    }

    [Fact]
    public void UpdateLastValue_WhenCalled_UpdatesValueTimeAndIncrementsVersion()
    {
        // Arrange
        Sensor sensor = new SensorBuilder().WithLastValue(10.0).Build();
        Guid initialVersion = sensor.Version;
        double newValue = 20.5;

        // Act
        sensor.UpdateLastValue(newValue);

        // Assert
        sensor.LastValue.Should().Be(newValue);
        sensor.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        sensor.Version.Should().NotBe(initialVersion);
    }

    [Fact]
    public void UpdateLastValue_WhenSensorNoDataOrDelayed_ResetsDelayAndSetsStateToActive()
    {
        // Arrange
        Sensor sensor = new SensorBuilder()
            .WithState(SensorState.NoData)
            .Build();
        sensor.MarkAsNoData();
        sensor.ClearDomainEvents();
        Guid initialVersion = sensor.Version;

        // Act
        sensor.UpdateLastValue(15.6);

        // Assert
        sensor.State.Should().Be(SensorState.Active);
        sensor.IsDataDelayed.Should().BeFalse();
        sensor.Version.Should().NotBe(initialVersion);
    }

    [Fact]
    public void SetState_WithDifferentState_UpdatesStateAndIncrementsVersion()
    {
        // Arrange
        Sensor sensor = new SensorBuilder().WithState(SensorState.Active).Build();
        Guid initialVersion = sensor.Version;

        // Act
        sensor.SetState(SensorState.Inactive);

        // Assert
        sensor.State.Should().Be(SensorState.Inactive);
        sensor.Version.Should().NotBe(initialVersion);
    }

    [Fact]
    public void SetState_WithSameState_DoesNotUpdateOrIncrementVersion()
    {
        // Arrange
        Sensor sensor = new SensorBuilder().WithState(SensorState.Active).Build();
        Guid initialVersion = sensor.Version;

        // Act
        sensor.SetState(SensorState.Active);

        // Assert
        sensor.State.Should().Be(SensorState.Active);
        sensor.Version.Should().Be(initialVersion);
    }

    [Fact]
    public void SetType_WithDifferentType_UpdatesTypeAndIncrementsVersion()
    {
        // Arrange
        Sensor sensor = new SensorBuilder().WithType(SensorType.Temperature).Build();
        Guid initialVersion = sensor.Version;

        // Act
        sensor.SetType(SensorType.Humidity);

        // Assert
        sensor.Type.Should().Be(SensorType.Humidity);
        sensor.Version.Should().NotBe(initialVersion);
    }

    [Fact]
    public void SetType_WithSameType_DoesNotUpdateOrIncrementVersion()
    {
        // Arrange
        Sensor sensor = new SensorBuilder().WithType(SensorType.Temperature).Build();
        Guid initialVersion = sensor.Version;

        // Act
        sensor.SetType(SensorType.Temperature);

        // Assert
        sensor.Type.Should().Be(SensorType.Temperature);
        sensor.Version.Should().Be(initialVersion);
    }

    [Fact]
    public void MarkAsNoData_ChangesStateRaisesEventAndIncrementsVersion()
    {
        // Arrange
        Sensor sensor = new SensorBuilder().WithState(SensorState.Active).Build();
        Guid initialVersion = sensor.Version;

        // Act
        sensor.MarkAsNoData();

        // Assert
        sensor.State.Should().Be(SensorState.NoData);
        sensor.IsDataDelayed.Should().BeTrue();
        sensor.Version.Should().NotBe(initialVersion);

        sensor.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<SensorNoDataDomainEvent>()
            .Which.SensorId.Should().Be(sensor.Id);
    }
}
