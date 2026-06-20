using Contracts.Constants;

namespace Device.Application.Features.Relays.Command.UpdateRelay;

internal sealed class UpdateRelayValidator
    : AbstractValidator<UpdateRelayCommand>
{
    public UpdateRelayValidator()
    {
        RuleFor(x => x.RelayId)
            .NotEmpty();

        RuleFor(x => x.ConnectionAddress)
            .NotEmpty();

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
    }
}
