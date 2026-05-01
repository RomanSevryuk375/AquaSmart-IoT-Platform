using Contracts.Enums;

namespace Notification.Domain.SpecificationParams;

public record NotificationSpecificationParams
{
    public Guid? UserId { get; init; }
    public Guid? EcosystemId { get; init; }
    public NotificationLevelEnum? Level { get; init; }
    public bool? IsRead { get; init; }
    public string? SearchTerm { get; init; } = string.Empty;
}
