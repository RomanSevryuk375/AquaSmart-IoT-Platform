namespace Device.Application.Features.RelayCommands.Command.SetRelayState;

internal sealed class SetRelayStateValidator
    : AbstractValidator<SetRelayStateCommand>
{
    public SetRelayStateValidator()
    {
        RuleFor(x => x.ControllerId)
            .NotEmpty();

        RuleFor(x => x.RelayId)
            .NotEmpty();

        RuleFor(x => x.TargetState)
            .NotEmpty();
    }
}
