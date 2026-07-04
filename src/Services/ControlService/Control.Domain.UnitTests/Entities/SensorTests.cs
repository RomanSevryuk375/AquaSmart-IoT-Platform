namespace Control.Domain.UnitTests.Entities;

public class SensorTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var controllerId = Guid.NewGuid();
        var ecosystemId = Guid.NewGuid();
        string name = "Pressure Sensor";
        SensorState state = SensorState.Active;
        SensorType type = SensorType.Pressure;
        DateTime createdAt = DateTime.UtcNow;

        // Act
        Result<Sensor> result = Sensor.Create(
            id, controllerId, ecosystemId, name, state, type, createdAt);

        // Assert
        result.IsSuccess.Should().BeTrue();
        Sensor sensor = result.Value;
        sensor.Id.Should().Be(id);
        sensor.ControllerId.Should().Be(controllerId);
        sensor.EcosystemId.Should().Be(ecosystemId);
        sensor.Name.Value.Should().Be(name);
        sensor.State.Should().Be(state);
        sensor.Type.Should().Be(type);
        sensor.LastValue.Should().Be(0.0);
        sensor.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void Create_WithInvalidName_ReturnsFailure()
    {
        // Act
        Result<Sensor> result = Sensor.Create(
            id: Guid.NewGuid(),
            controllerId: Guid.NewGuid(),
            ecosystemId: Guid.NewGuid(),
            rawName: "",
            SensorState.Active,
            SensorType.Pressure,
            createdAt: DateTime.UtcNow);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Name.Invalid");
    }

    [Fact]
    public void SetState_WithNewState_UpdatesStateAndIncrementsVersion()
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
    public void SetState_WithSameState_DoesNotIncrementVersion()
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
    public void SetLastValue_UpdatesLastValueAndIncrementsVersion()
    {
        // Arrange
        Sensor sensor = new SensorBuilder().Build();
        Guid initialVersion = sensor.Version;
        double newValue = 8.2;

        // Act
        sensor.SetLastValue(newValue);

        // Assert
        sensor.LastValue.Should().Be(newValue);
        sensor.Version.Should().NotBe(initialVersion);
    }

    [Fact]
    public void SetType_WithNewType_UpdatesTypeAndIncrementsVersion()
    {
        // Arrange
        Sensor sensor = new SensorBuilder().WithType(SensorType.Pressure).Build();
        Guid initialVersion = sensor.Version;

        // Act
        sensor.SetType(SensorType.Temperature);

        // Assert
        sensor.Type.Should().Be(SensorType.Temperature);
        sensor.Version.Should().NotBe(initialVersion);
    }

    [Fact]
    public void SetType_WithSameType_DoesNotIncrementVersion()
    {
        // Arrange
        Sensor sensor = new SensorBuilder().WithType(SensorType.Pressure).Build();
        Guid initialVersion = sensor.Version;

        // Act
        sensor.SetType(SensorType.Pressure);

        // Assert
        sensor.Type.Should().Be(SensorType.Pressure);
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
    public void SetName_WithInvalidName_ReturnsFailureAndDoesNotIncrementVersion()
    {
        // Arrange
        Sensor sensor = new SensorBuilder().WithName("Old Name").Build();
        Guid initialVersion = sensor.Version;

        // Act
        Result result = sensor.SetName("");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Name.Invalid");
        sensor.Name.Value.Should().Be("Old Name");
        sensor.Version.Should().Be(initialVersion);
    }
}
