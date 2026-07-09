// Ignore Spelling: Validator

using Contracts.Constants;
using FluentValidation;

namespace Notification.Application.Features.Reminders.Commands.UpdateReminder;

public sealed class UpdateReminderValidator : AbstractValidator<UpdateReminderCommand>
{
    public UpdateReminderValidator()
    {
        RuleFor(x => x.ReminderId)
            .NotEmpty();

        RuleFor(x => x.TaskName)
            .NotEmpty()
            .MaximumLength(ReminderConstants.NameLength);

        RuleFor(x => x.IntervalDays)
            .GreaterThan(0);
    }
}
