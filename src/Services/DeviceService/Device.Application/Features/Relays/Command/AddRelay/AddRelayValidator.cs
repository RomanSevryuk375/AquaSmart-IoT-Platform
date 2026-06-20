using Contracts.Constants;

namespace Device.Application.Features.Relays.Command.AddRelay;

internal sealed class AddRelayValidator
    : AbstractValidator<AddRelayCommand>
{
    public AddRelayValidator()
    {
        RuleFor(x => x.ConnectionAddress)
            .NotEmpty();

        RuleFor(x => x.PowerSensorId)
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

        RuleFor(x => x.IsNormalyOpen)
            .NotEmpty();

        RuleFor(x => x.Purpose)
            .NotEmpty()
            .IsInEnum();

        RuleFor(x => x.IsActive)
            .NotEmpty();

        RuleFor(x => x.IsManual)
            .NotEmpty();
    }
}
