using Contracts.Abstractions;
using Contracts.Enums;
using Contracts.Results;
using Notification.Application.Features.Notifications.Queries.Shared;

namespace Notification.Application.Features.Notifications.Queries.GetAllNotifications;

public sealed record GetAllNotificationsQuery
    : IQuery<Result<IReadOnlyList<NotificationDto>>>
{
    public Guid UserId { get; init; }
    public Guid? EcosystemId { get; init; }
    public NotificationLevel? Level { get; init; }
    public bool? IsRead { get; init; }
    public string? SearchTerm { get; init; }
    public int Skip { get; init; } = 0;
    public int Take { get; init; } = 10;
}
