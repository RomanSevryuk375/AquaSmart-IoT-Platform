using Contracts.Enums;

namespace Notification.Application.DTOs.Notification;

public record NotificationResponseDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid? EcosystemId { get; init; }
    public NotificationLevel Level { get; init; }
    public string Message { get; init; } = string.Empty;
    public bool IsRead { get; init; }
    public DateTime CreatedAt { get; init; }
}
