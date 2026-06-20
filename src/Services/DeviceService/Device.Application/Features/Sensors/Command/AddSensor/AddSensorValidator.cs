using Contracts.Constants;

namespace Device.Application.Features.Sensors.Command.AddSensor;

internal sealed class AddSensorValidator
    : AbstractValidator<AddSensorCommand>
{
    public AddSensorValidator()
    {
        RuleFor(x => x.ControllerId)
           .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(CommonConstants.NameLength);

        RuleFor(x => x.ConnectionProtocol)
            .NotEmpty()
            .IsInEnum();

        RuleFor(x => x.ConnectionAddress)
            .NotEmpty()
            .MaximumLength(CommonConstants.ConnectionAddressLength);

        RuleFor(x => x.Type)
            .NotEmpty()
            .IsInEnum();

        RuleFor(x => x.Unit)
            .NotEmpty()
            .MaximumLength(SensorConstants.UnitLength);
    }
}
