using Contracts.Enums;

namespace Notification.Application.DTOs.Notification;

public record NotificationFilterDto
{
    public Guid? UserId { get; init; }
    public Guid? EcosystemId { get; init; }
    public NotificationLevel? Level { get; init; }
    public bool? IsRead { get; init; }
    public string? SearchTerm { get; init; } = string.Empty;
}
