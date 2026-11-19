using BuildingBlocks.Domain.Abstractions;

namespace Billing.Domain.Events;

public sealed record InvoicePaidDomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid CustomerId { get; init; }
    public Guid InvoiceId { get; init; }
    public string InvoiceNumber { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public DateTime PaidAt { get; init; }
}
