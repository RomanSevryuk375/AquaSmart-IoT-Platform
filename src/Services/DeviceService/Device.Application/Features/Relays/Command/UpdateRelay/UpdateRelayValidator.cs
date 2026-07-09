// Ignore Spelling: Validator

namespace Device.Application.Features.Relays.Command.UpdateRelay;

internal sealed class UpdateRelayValidator : AbstractValidator<UpdateRelayCommand>
{
    public UpdateRelayValidator()
    {
        RuleFor(x => x.RelayId)
            .NotEmpty();

        RuleFor(x => x.ControllerId)
            .NotEmpty();

        RuleFor(x => x.ConnectionProtocol)
            .IsInEnum();

        RuleFor(x => x.ConnectionAddress)
            .NotEmpty()
            .MaximumLength(CommonConstants.ConnectionAddressLength);

        RuleFor(x => x.Purpose)
            .IsInEnum();
    }
}
