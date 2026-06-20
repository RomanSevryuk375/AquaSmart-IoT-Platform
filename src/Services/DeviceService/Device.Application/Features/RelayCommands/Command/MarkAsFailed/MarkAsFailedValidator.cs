// Ignore Spelling: Validator

namespace Device.Application.Features.RelayCommands.Command.MarkAsFailed;

internal sealed class MarkAsFailedValidator
    : AbstractValidator<MarkAsFailedCommand>
{
    public MarkAsFailedValidator()
    {
        RuleFor(x => x.CommandId)
            .NotEmpty();

        RuleFor(x => x.DeviceToken)
            .NotEmpty();

        RuleFor(x => x.ErrorMessage)
            .NotEmpty();
    }
}
