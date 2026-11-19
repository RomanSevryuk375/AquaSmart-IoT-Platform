using BuildingBlocks.Domain.Abstractions;

namespace Billing.Domain.Events;

public sealed record SubscriptionActivatedDomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid CustomerId { get; init; }
    public Guid SubscriptionId { get; init; }
    public Guid PlanId { get; init; }
    public DateTime PeriodEnd { get; init; }
}
