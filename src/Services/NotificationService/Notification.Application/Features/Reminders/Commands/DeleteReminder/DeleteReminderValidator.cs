using FluentValidation;

namespace Notification.Application.Features.Reminders.Commands.DeleteReminder;

public sealed class DeleteReminderValidator : AbstractValidator<DeleteReminderCommand>
{
    public DeleteReminderValidator()
    {
        RuleFor(x => x.ReminderId).NotEmpty();
    }
}
