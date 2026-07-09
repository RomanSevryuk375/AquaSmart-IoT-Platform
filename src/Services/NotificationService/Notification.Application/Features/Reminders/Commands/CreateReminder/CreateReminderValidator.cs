// Ignore Spelling: Validator

using Contracts.Constants;
using FluentValidation;

namespace Notification.Application.Features.Reminders.Commands.CreateReminder;

public sealed class CreateReminderValidator
    : AbstractValidator<CreateReminderCommand>
{
    public CreateReminderValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();
        RuleFor(x => x.EcosystemId)
            .NotEmpty();

        RuleFor(x => x.TaskName)
            .NotEmpty()
            .MaximumLength(ReminderConstants.NameLength);

        RuleFor(x => x.IntervalDays)
            .GreaterThan(0);
    }
}
