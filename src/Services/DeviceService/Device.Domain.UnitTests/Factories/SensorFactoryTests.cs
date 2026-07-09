namespace Device.Domain.UnitTests.Factories;

public class SensorFactoryTests
{
    [Fact]
    public void CreateSensor_WithTemperatureType_ReturnsTemperatureSensorAndRaisesEvent()
    {
        // Act
        Result<Sensor> result = SensorFactory.CreateSensor(
            TestConstants.SensorId,
            TestConstants.ControllerId,
            TestConstants.UserId,
            TestConstants.ValidDeviceName,
            TestConstants.ValidProtocol,
            TestConstants.ValidI2cAddress,
            SensorType.Temperature);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<TemperatureSensor>();

        Sensor sensor = result.Value;
        sensor.Type.Should().Be(SensorType.Temperature);
        sensor.Unit.Should().Be(UnitConstants.Celsius);
        sensor.Name.Value.Should().Be(TestConstants.ValidDeviceName);
        sensor.State.Should().Be(SensorState.NoData);

        sensor.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<SensorCreatedDomainEvent>()
            .Which.Should().Match<SensorCreatedDomainEvent>(e =>
                e.SensorId == TestConstants.SensorId &&
                e.ControllerId == TestConstants.ControllerId &&
                e.Name == TestConstants.ValidDeviceName &&
                e.Type == SensorType.Temperature &&
                e.Unit == UnitConstants.Celsius &&
                e.State == SensorState.NoData);
    }

    [Fact]
    public void CreateSensor_WithHumidityType_ReturnsHumiditySensor()
    {
        // Act
        Result<Sensor> result = SensorFactory.CreateSensor(
            TestConstants.SensorId,
            TestConstants.ControllerId,
            TestConstants.UserId,
            TestConstants.ValidDeviceName,
            TestConstants.ValidProtocol,
            TestConstants.ValidI2cAddress,
            SensorType.Humidity);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<HumiditySensor>();
        result.Value.Type.Should().Be(SensorType.Humidity);
        result.Value.Unit.Should().Be("%");
    }

    [Fact]
    public void CreateSensor_WithPressureType_ReturnsPressureSensor()
    {
        // Act
        Result<Sensor> result = SensorFactory.CreateSensor(
            TestConstants.SensorId,
            TestConstants.ControllerId,
            TestConstants.UserId,
            TestConstants.ValidDeviceName,
            TestConstants.ValidProtocol,
            TestConstants.ValidI2cAddress,
            SensorType.Pressure);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<PressureSensor>();
        result.Value.Type.Should().Be(SensorType.Pressure);
        result.Value.Unit.Should().Be("Pa");
    }

    [Fact]
    public void CreateSensor_WithVoltageType_ReturnsVoltageSensor()
    {
        // Act
        Result<Sensor> result = SensorFactory.CreateSensor(
            TestConstants.SensorId,
            TestConstants.ControllerId,
            TestConstants.UserId,
            TestConstants.ValidDeviceName,
            TestConstants.ValidProtocol,
            TestConstants.ValidI2cAddress,
            SensorType.Voltage);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<VoltageSensor>();
        result.Value.Type.Should().Be(SensorType.Voltage);
        result.Value.Unit.Should().Be("V");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateSensor_WithInvalidName_ReturnsValidationFailure(string invalidName)
    {
        // Act
        Result<Sensor> result = SensorFactory.CreateSensor(
            TestConstants.SensorId,
            TestConstants.ControllerId,
            TestConstants.UserId,
            invalidName,
            TestConstants.ValidProtocol,
            TestConstants.ValidI2cAddress,
            SensorType.Temperature);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain(CommonErrors.NameEmpty);
    }

    [Fact]
    public void CreateSensor_WithInvalidAddress_ReturnsValidationFailure()
    {
        // Arrange
        string invalidAddress = "0xGG";

        // Act
        Result<Sensor> result = SensorFactory.CreateSensor(
            TestConstants.SensorId,
            TestConstants.ControllerId,
            TestConstants.UserId,
            TestConstants.ValidDeviceName,
            TestConstants.ValidProtocol,
            invalidAddress,
            SensorType.Temperature);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain(CommonErrors.I2cInvalidFormat);
    }

    [Fact]
    public void CreateSensor_WithEmptyId_ReturnsValidationFailure()
    {
        // Act
        Result<Sensor> result = SensorFactory.CreateSensor(
            Guid.Empty,
            TestConstants.ControllerId,
            TestConstants.UserId,
            TestConstants.ValidDeviceName,
            TestConstants.ValidProtocol,
            TestConstants.ValidI2cAddress,
            SensorType.Temperature);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain(CommonErrors.IdEmpty);
    }

    [Fact]
    public void CreateSensor_WithEmptyControllerId_ReturnsValidationFailure()
    {
        // Act
        Result<Sensor> result = SensorFactory.CreateSensor(
            TestConstants.SensorId,
            Guid.Empty,
            TestConstants.UserId,
            TestConstants.ValidDeviceName,
            TestConstants.ValidProtocol,
            TestConstants.ValidI2cAddress,
            SensorType.Temperature);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain(ControllerErrors.ControllerIdEmpty);
    }

    [Fact]
    public void CreateSensor_WithEmptyUserId_ReturnsValidationFailure()
    {
        // Act
        Result<Sensor> result = SensorFactory.CreateSensor(
            TestConstants.SensorId,
            TestConstants.ControllerId,
            Guid.Empty,
            TestConstants.ValidDeviceName,
            TestConstants.ValidProtocol,
            TestConstants.ValidI2cAddress,
            SensorType.Temperature);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain(UserErrors.UserIdEmpty);
    }

    [Fact]
    public void CreateSensor_WithUnsupportedType_ReturnsValidationFailure()
    {
        // Arrange
        var unsupportedType = (SensorType)999;

        // Act
        Result<Sensor> result = SensorFactory.CreateSensor(
            TestConstants.SensorId,
            TestConstants.ControllerId,
            TestConstants.UserId,
            TestConstants.ValidDeviceName,
            TestConstants.ValidProtocol,
            TestConstants.ValidI2cAddress,
            unsupportedType);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain(SensorErrors.InvalidType);
    }
}
