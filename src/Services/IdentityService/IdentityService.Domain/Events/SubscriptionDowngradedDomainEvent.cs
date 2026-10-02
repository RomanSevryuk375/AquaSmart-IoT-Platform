using BuildingBlocks.Domain.Abstractions;

namespace IdentityService.Domain.Events;

public sealed record SubscriptionDowngradedDomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid UserId { get; init; }
    public Guid NewSubscriptionId { get; init; }
}
