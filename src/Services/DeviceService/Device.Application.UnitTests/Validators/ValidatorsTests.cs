using BuildingBlocks.Domain.Enums;
using Device.Application.Features.Controllers.Command.AddController;
using Device.Application.Features.Controllers.Command.PingController;
using Device.Application.Features.Controllers.Command.ToggleCommandState;
using Device.Application.Features.Controllers.Command.UpdateController;
using Device.Application.Features.RelayCommands.Command.MarkAsCompleted;
using Device.Application.Features.RelayCommands.Command.MarkAsFailed;
using Device.Application.Features.RelayCommands.Command.SetRelayState;
using Device.Application.Features.RelayCommands.Command.ToggleRelayMode;
using Device.Application.Features.RelayCommands.Command.ToggleRelayState;
using Device.Application.Features.Relays.Command.AddRelay;
using Device.Application.Features.Relays.Command.SetRelayPowerSensor;
using Device.Application.Features.Relays.Command.UpdateRelay;
using Device.Application.Features.Sensors.Command.AddSensor;
using Device.Application.Features.Sensors.Command.SetSensorState;
using Device.Application.Features.Sensors.Command.UpdateSensor;

namespace Device.Application.UnitTests.Validators;

public class ValidatorsTests
{
    [Fact]
    public void AddControllerValidator_ValidAndInvalid()
    {
        var validator = new AddControllerValidator();

        var valid = new AddControllerCommand
        {
            UserId = Guid.NewGuid(),
            MacAddress = TestConstants.ValidMacAddress,
            Name = "Valid Device"
        };
        validator.Validate(valid).IsValid.Should().BeTrue();

        var invalid = new AddControllerCommand
        {
            UserId = Guid.NewGuid(),
            MacAddress = "invalid-mac",
            Name = ""
        };
        validator.Validate(invalid).IsValid.Should().BeFalse();
    }

    [Fact]
    public void UpdateControllerValidator_ValidAndInvalid()
    {
        var validator = new UpdateControllerValidator();

        var valid = new UpdateControllerCommand
        {
            UserId = Guid.NewGuid(),
            ControllerId = Guid.NewGuid(),
            MacAddress = TestConstants.ValidMacAddress,
            Name = "Valid Device"
        };
        validator.Validate(valid).IsValid.Should().BeTrue();

        var invalid = new UpdateControllerCommand
        {
            UserId = Guid.Empty,
            MacAddress = "invalid-mac",
            Name = ""
        };
        validator.Validate(invalid).IsValid.Should().BeFalse();
    }

    [Fact]
    public void PingControllerValidator_ValidAndInvalid()
    {
        var validator = new PingControllerValidator();

        var valid = new PingControllerCommand
        {
            ControllerId = Guid.NewGuid(),
            DeviceToken = "valid_token"
        };
        validator.Validate(valid).IsValid.Should().BeTrue();

        var invalid = new PingControllerCommand
        {
            ControllerId = Guid.Empty,
            DeviceToken = ""
        };
        validator.Validate(invalid).IsValid.Should().BeFalse();
    }

    [Fact]
    public void ToggleControllerStateValidator_ValidAndInvalid()
    {
        var validator = new ToggleControllerStateValidator();

        var valid = new ToggleControllerStateCommand { UserId = Guid.NewGuid(), ControllerId = Guid.NewGuid() };
        validator.Validate(valid).IsValid.Should().BeTrue();

        var invalid = new ToggleControllerStateCommand { UserId = Guid.Empty, ControllerId = Guid.NewGuid() };
        validator.Validate(invalid).IsValid.Should().BeFalse();
    }

    [Fact]
    public void AddRelayValidator_ValidAndInvalid()
    {
        var validator = new AddRelayValidator();

        var valid = new AddRelayCommand
        {
            Name = "Relay",
            ConnectionProtocol = ConnectionProtocol.Digital,
            ConnectionAddress = "D1",
            Purpose = RelayPurpose.Pump
        };
        validator.Validate(valid).IsValid.Should().BeTrue();

        var invalid = new AddRelayCommand
        {
            Name = "",
            ConnectionAddress = ""
        };
        validator.Validate(invalid).IsValid.Should().BeFalse();
    }

    [Fact]
    public void UpdateRelayValidator_ValidAndInvalid()
    {
        var validator = new UpdateRelayValidator();

        var valid = new UpdateRelayCommand
        {
            RelayId = Guid.NewGuid(),
            ControllerId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            ConnectionProtocol = ConnectionProtocol.Digital,
            ConnectionAddress = "D1",
            Purpose = RelayPurpose.Pump
        };
        validator.Validate(valid).IsValid.Should().BeTrue();

        var invalid = new UpdateRelayCommand
        {
            RelayId = Guid.Empty,
            ConnectionAddress = ""
        };
        validator.Validate(invalid).IsValid.Should().BeFalse();
    }

    [Fact]
    public void SetRelayPowerSensorValidator_ValidAndInvalid()
    {
        var validator = new SetRelayPowerSensorValidator();

        var valid = new SetRelayPowerSensorCommand
        {
            RelayId = Guid.NewGuid(),
            PowerSensorId = Guid.NewGuid(),
            UserId = Guid.NewGuid()
        };
        validator.Validate(valid).IsValid.Should().BeTrue();

        var invalid = new SetRelayPowerSensorCommand
        {
            RelayId = Guid.Empty,
            PowerSensorId = Guid.Empty
        };
        validator.Validate(invalid).IsValid.Should().BeFalse();
    }

    [Fact]
    public void SetRelayStateValidator_ValidAndInvalid()
    {
        var validator = new SetRelayStateValidator();

        var valid = new SetRelayStateCommand
        {
            ControllerId = Guid.NewGuid(),
            RelayId = Guid.NewGuid(),
            TargetState = true
        };
        validator.Validate(valid).IsValid.Should().BeTrue();

        var invalid = new SetRelayStateCommand
        {
            ControllerId = Guid.Empty,
            RelayId = Guid.Empty
        };
        validator.Validate(invalid).IsValid.Should().BeFalse();
    }

    [Fact]
    public void ToggleRelayStateValidator_ValidAndInvalid()
    {
        var validator = new ToggleRelayStateValidator();

        var valid = new ToggleRelayStateCommand { RelayId = Guid.NewGuid() };
        validator.Validate(valid).IsValid.Should().BeTrue();

        var invalid = new ToggleRelayStateCommand { RelayId = Guid.Empty };
        validator.Validate(invalid).IsValid.Should().BeFalse();
    }

    [Fact]
    public void ToggleRelayModeValidator_ValidAndInvalid()
    {
        var validator = new ToggleRelayModeValidator();

        var valid = new ToggleRelayModeCommand { RelayId = Guid.NewGuid() };
        validator.Validate(valid).IsValid.Should().BeTrue();

        var invalid = new ToggleRelayModeCommand { RelayId = Guid.Empty };
        validator.Validate(invalid).IsValid.Should().BeFalse();
    }

    [Fact]
    public void MarkAsCompletedValidator_ValidAndInvalid()
    {
        var validator = new MarkAsCompletedValidator();

        var valid = new MarkAsCompletedCommand { CommandId = Guid.NewGuid(), DeviceToken = "token" };
        validator.Validate(valid).IsValid.Should().BeTrue();

        var invalid = new MarkAsCompletedCommand { CommandId = Guid.Empty, DeviceToken = "" };
        validator.Validate(invalid).IsValid.Should().BeFalse();
    }

    [Fact]
    public void MarkAsFailedValidator_ValidAndInvalid()
    {
        var validator = new MarkAsFailedValidator();

        var valid = new MarkAsFailedCommand
        {
            CommandId = Guid.NewGuid(),
            DeviceToken = "token",
            ErrorMessage = "timeout"
        };
        validator.Validate(valid).IsValid.Should().BeTrue();

        var invalid = new MarkAsFailedCommand
        {
            CommandId = Guid.Empty,
            DeviceToken = "",
            ErrorMessage = ""
        };
        validator.Validate(invalid).IsValid.Should().BeFalse();
    }

    [Fact]
    public void AddSensorValidator_ValidAndInvalid()
    {
        var validator = new AddSensorValidator();

        var valid = new AddSensorCommand
        {
            ControllerId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "Water Temp",
            ConnectionProtocol = ConnectionProtocol.I2C,
            ConnectionAddress = TestConstants.ValidI2cAddress,
            Type = SensorType.Temperature
        };
        validator.Validate(valid).IsValid.Should().BeTrue();

        var invalid = new AddSensorCommand
        {
            Name = "",
            ConnectionAddress = ""
        };
        validator.Validate(invalid).IsValid.Should().BeFalse();
    }

    [Fact]
    public void UpdateSensorValidator_ValidAndInvalid()
    {
        var validator = new UpdateSensorValidator();

        var valid = new UpdateSensorCommand
        {
            SensorId = Guid.NewGuid(),
            ControllerId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "Sens",
            ConnectionProtocol = ConnectionProtocol.I2C,
            ConnectionAddress = TestConstants.ValidI2cAddress
        };
        validator.Validate(valid).IsValid.Should().BeTrue();

        var invalid = new UpdateSensorCommand
        {
            SensorId = Guid.Empty,
            Name = ""
        };
        validator.Validate(invalid).IsValid.Should().BeFalse();
    }

    [Fact]
    public void SetSensorStateValidator_ValidAndInvalid()
    {
        var validator = new SetSensorStateValidator();

        var valid = new SetSensorStateCommand
        {
            SensorId = Guid.NewGuid(),
            SensorState = SensorState.Active
        };
        validator.Validate(valid).IsValid.Should().BeTrue();

        var invalid = new SetSensorStateCommand
        {
            SensorId = Guid.Empty
        };
        validator.Validate(invalid).IsValid.Should().BeFalse();
    }
}
