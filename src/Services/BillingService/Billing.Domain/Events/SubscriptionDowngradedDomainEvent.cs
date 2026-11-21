using BuildingBlocks.Domain.Abstractions;

namespace Billing.Domain.Events;

public sealed record SubscriptionDowngradedDomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid CustomerId { get; init; }
    public Guid SubscriptionId { get; init; }
    public string Reason { get; init; } = string.Empty;
}
