// Ignore Spelling: Dto

using Contracts.Enums;

namespace Notification.Application.Features.Notifications.Queries.Shared;

public sealed record NotificationDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid? EcosystemId { get; init; }
    public NotificationLevel Level { get; init; }
    public string Message { get; init; } = string.Empty;
    public bool IsRead { get; init; }
    public DateTime CreatedAt { get; init; }
}
