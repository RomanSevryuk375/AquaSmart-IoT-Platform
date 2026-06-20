// Ignore Spelling: Validator

using Contracts.Constants;

namespace Device.Application.Features.Sensors.Command.UpdateSensor;

internal sealed class UpdateSensorValidator
    : AbstractValidator<UpdateSensorCommand>
{
    public UpdateSensorValidator()
    {
        RuleFor(x => x.ControllerId)
            .NotEmpty();

        RuleFor(x => x.ConnectionProtocol)
            .NotEmpty()
            .IsInEnum();

        RuleFor(x => x.ConnectionAddress)
            .NotEmpty()
            .MaximumLength(CommonConstants.ConnectionAddressLength);
    }
}
