using BuildingBlocks.Domain.Abstractions;

namespace Billing.Domain.Events;

public sealed record SubscriptionCanceledDomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid CustomerId { get; init; }
    public Guid SubscriptionId { get; init; }
    public bool CancelAtPeriodEnd { get; init; }
    public DateTime CanceledAt { get; init; }
}
