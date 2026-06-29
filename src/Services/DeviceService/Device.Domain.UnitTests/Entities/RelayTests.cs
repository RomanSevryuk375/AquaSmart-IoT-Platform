using Contracts.Constants;
using Contracts.Enums;
using Contracts.Results;
using Device.Domain.Entities;
using Device.Domain.Entities.Sensors;
using Device.Domain.Events.RelayEvents;
using Device.TestShared.Builders;
using Device.TestShared.Constants;
using FluentAssertions;

namespace Device.Domain.UnitTests.Entities;

public class RelayTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccessAndRaisesEvent()
    {
        // Act
        Result<Relay> result = Relay.Create(
            TestConstants.RelayId,
            TestConstants.ControllerId,
            TestConstants.UserId,
            powerSensorId: null,
            TestConstants.ValidDeviceName,
            ConnectionProtocol.Digital,
            TestConstants.ValidDigitalAddress,
            isNormallyOpen: true,
            RelayPurpose.Pump,
            isActive: false,
            isManual: true);

        // Assert
        result.IsSuccess.Should().BeTrue();
        Relay relay = result.Value;

        relay.Id.Should().Be(TestConstants.RelayId);
        relay.ControllerId.Should().Be(TestConstants.ControllerId);
        relay.Name.Value.Should().Be(TestConstants.ValidDeviceName);
        relay.ConnectionAddress.Protocol.Should().Be(ConnectionProtocol.Digital);
        relay.ConnectionAddress.Address.Should().Be(TestConstants.ValidDigitalAddress);
        relay.Purpose.Should().Be(RelayPurpose.Pump);

        relay.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<RelayCreatedDomainEvent>()
            .Which.Should().Match<RelayCreatedDomainEvent>(e =>
                e.RelayId == TestConstants.RelayId &&
                e.ControllerId == TestConstants.ControllerId &&
                e.Name == TestConstants.ValidDeviceName);
    }

    [Fact]
    public void Create_WithInvalidAddress_ReturnsValidationFailure()
    {
        // Act
        Result<Relay> result = Relay.Create(
            TestConstants.RelayId,
            TestConstants.ControllerId,
            TestConstants.UserId,
            powerSensorId: null,
            TestConstants.ValidDeviceName,
            ConnectionProtocol.I2C,
            "invalid",
            isNormallyOpen: true,
            RelayPurpose.Pump,
            isActive: false,
            isManual: true);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain(CommonErrors.I2cInvalidFormat);
    }

    [Fact]
    public void Update_WithValidData_UpdatesPropertiesAndRaisesEvent()
    {
        Relay relay = new RelayBuilder().Build();

        var newControllerId = Guid.NewGuid();
        ConnectionProtocol newProtocol = ConnectionProtocol.Analog;
        string newAddress = "A0";
        RelayPurpose newPurpose = RelayPurpose.Light;

        // Act
        Result result = relay.Update(newControllerId, newProtocol, newAddress, newPurpose, isNormallyOpen: false);

        // Assert
        result.IsSuccess.Should().BeTrue();
        relay.ControllerId.Should().Be(newControllerId);
        relay.ConnectionAddress.Protocol.Should().Be(newProtocol);
        relay.ConnectionAddress.Address.Should().Be(newAddress);
        relay.Purpose.Should().Be(newPurpose);

        relay.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<RelayUpdatedDomainEvent>()
            .Which.ControllerId.Should().Be(newControllerId);
    }

    [Fact]
    public void SetPowerSensor_WithVoltageSensor_SetsIdAndRaisesEvent()
    {
        // Arrange
        Relay relay = new RelayBuilder().Build();

        Sensor voltageSensor = new SensorBuilder()
            .WithControllerId(TestConstants.ControllerId)
            .WithType(SensorType.Voltage)
            .Build();

        // Act
        Result result = relay.SetPowerSensor(voltageSensor);

        // Assert
        result.IsSuccess.Should().BeTrue();
        relay.PowerSensorId.Should().Be(voltageSensor.Id);

        relay.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<SetRelayPowerSensorDomainEvent>()
            .Which.PowerSensorId.Should().Be(voltageSensor.Id);
    }

    [Fact]
    public void SetPowerSensor_WithTemperatureSensor_ReturnsConflictFailure()
    {
        // Arrange
        Relay relay = new RelayBuilder().Build();

        Sensor tempSensor = new SensorBuilder()
            .WithControllerId(TestConstants.ControllerId)
            .WithType(SensorType.Temperature)
            .Build();

        // Act
        Result result = relay.SetPowerSensor(tempSensor);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain(RelayErrors.InvalidPowerSensorType);
        relay.PowerSensorId.Should().BeNull();
        relay.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void SetState_WithNewState_UpdatesStateAndRaisesEvent()
    {
        // Arrange
        Relay relay = new RelayBuilder().AsActive(false).Build();
        bool targetState = true;

        // Act
        relay.SetState(targetState);

        // Assert
        relay.IsActive.Should().Be(targetState);
        relay.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<RelayStateChangedDomainEvent>()
            .Which.TargetState.Should().BeTrue();
    }

    [Fact]
    public void SetMode_WithNewMode_UpdatesModeAndRaisesEvent()
    {
        // Arrange 
        Relay relay = new RelayBuilder().Build();
        bool targetMode = false;

        // Act
        relay.SetMode(targetMode);

        // Assert
        relay.IsManual.Should().Be(targetMode);
        relay.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<RelayModeChangedDomainEvent>()
            .Which.IsManual.Should().BeFalse();
    }
}
