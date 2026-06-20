namespace Device.Application.Features.RelayCommands.Command.MarkAsCompleted;

internal sealed class MarkAsCompletedValidator
    : AbstractValidator<MarkAsCompletedCommand>
{
    public MarkAsCompletedValidator()
    {
        RuleFor(x => x.CommandId)
            .NotEmpty();

        RuleFor(x => x.DeviceToken)
            .NotEmpty();
    }
}
