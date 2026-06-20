namespace Device.Application.Features.RelayCommands.Command.ToggleRelayState;

internal sealed class ToggleRelayStateValidator
    : AbstractValidator<ToggleRelayStateCommand>
{
    public ToggleRelayStateValidator()
    {
        RuleFor(x => x.RelayId)
            .NotEmpty();
    }
}
