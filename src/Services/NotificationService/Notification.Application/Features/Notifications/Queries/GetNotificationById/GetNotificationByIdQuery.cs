using Contracts.Abstractions;
using Contracts.Results;
using Notification.Application.Features.Notifications.Queries.Shared;

namespace Notification.Application.Features.Notifications.Queries.GetNotificationById;

public sealed record GetNotificationByIdQuery
    : IQuery<Result<NotificationDto>>
{
    public Guid NotificationId { get; init; }
    public Guid UserId { get; init; }
}
