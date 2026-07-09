// Ignore Spelling: Validator

using FluentValidation;

namespace Notification.Application.Features.Notifications.Commands.MarkNotificationAsRead;

public sealed class MarkNotificationAsReadValidator : AbstractValidator<MarkNotificationAsReadCommand>
{
    public MarkNotificationAsReadValidator()
    {
        RuleFor(x => x.NotificationId)
            .NotEmpty();

        RuleFor(x => x.UserId)
            .NotEmpty();
    }
}
