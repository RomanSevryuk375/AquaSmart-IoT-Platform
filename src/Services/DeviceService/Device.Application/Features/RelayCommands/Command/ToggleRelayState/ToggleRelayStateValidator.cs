namespace Device.Application.Features.RelayCommands.Command.ToggleRelayState;

internal sealed class ToggleRelayStateInvalidator
    : AbstractValidator<ToggleRelayStateCommand>
{
    public ToggleRelayStateInvalidator()
    {
        RuleFor(x => x.RelayId)
            .NotEmpty();
    }
}
