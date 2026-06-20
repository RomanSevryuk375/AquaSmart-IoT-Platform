namespace Device.Application.Features.Sensors.Command.SetSensorState;

internal sealed class SetSensorStateValidator
    : AbstractValidator<SetSensorStateCommand>
{
    public SetSensorStateValidator()
    {
        RuleFor(x => x.SensorId)
            .NotEmpty();

        RuleFor(x => x.SensorState)
            .IsInEnum()
            .NotEmpty();
    }
}
