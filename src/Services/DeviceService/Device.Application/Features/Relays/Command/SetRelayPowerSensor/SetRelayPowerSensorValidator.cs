// Ignore Spelling: Validator

namespace Device.Application.Features.Relays.Command.SetRelayPowerSensor;

internal sealed class SetRelayPowerSensorValidator
    : AbstractValidator<SetRelayPowerSensorCommand>
{
    public SetRelayPowerSensorValidator()
    {
        RuleFor(x => x.PowerSensorId)
            .NotEmpty();

        RuleFor(x => x.RelayId)
            .NotEmpty();
    }
}
