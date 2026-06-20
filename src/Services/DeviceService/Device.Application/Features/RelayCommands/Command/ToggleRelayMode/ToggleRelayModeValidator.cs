namespace Device.Application.Features.RelayCommands.Command.ToggleRelayMode;

internal sealed class ToggleRelayModeValidator
    : AbstractValidator<ToggleRelayModeCommand>
{
    public ToggleRelayModeValidator()
    {
        RuleFor(x => x.RelayId)
            .NotEmpty();
    }
}
