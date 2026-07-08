using FluentValidation;

namespace Notification.Application.Features.Notifications.Queries.GetNotificationById;

public sealed class GetNotificationByIdValidator : AbstractValidator<GetNotificationByIdQuery>
{
    public GetNotificationByIdValidator()
    {
        RuleFor(x => x.NotificationId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}
