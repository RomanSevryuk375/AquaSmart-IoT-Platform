using BuildingBlocks.Domain.Abstractions;

namespace Billing.Domain.Events;

public sealed record PaymentFailedDomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid CustomerId { get; init; }
    public Guid? SubscriptionId { get; init; }
    public Guid PaymentId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public int AttemptNumber { get; init; }
    public DateTime? NextRetryDate { get; init; }
}
