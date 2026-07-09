using FluentValidation;

namespace Notification.Application.Features.Reminders.Commands.CompleteReminder;

public sealed class CompleteReminderValidator : AbstractValidator<CompleteReminderCommand>
{
    public CompleteReminderValidator()
    {
        RuleFor(x => x.ReminderId).NotEmpty();
    }
}
