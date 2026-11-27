using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Common;
using BuildingBlocks.Domain.Results;
using BuildingBlocks.Domain.ValueObjects;

namespace Billing.Domain.Entities;

public sealed class InvoiceItem : IEntity
{
    public const int MaxDescriptionLength = 256;

    private InvoiceItem(
        Guid id,
        Guid invoiceId,
        string description,
        int quantity,
        Money unitPrice,
        DateRange? period,
        DateTime createdAt)
    {
        Id = id;
        InvoiceId = invoiceId;
        Description = description;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TotalAmount = Money.Parse(quantity * unitPrice.Amount);
        Period = period;
        CreatedAt = createdAt;
    }

#pragma warning disable CS8618
    private InvoiceItem() { }
#pragma warning restore CS8618

    public Guid Id { get; private set; }
    public Guid InvoiceId { get; private set; }
    public string Description { get; private set; }
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; }
    public Money TotalAmount { get; private set; }
    public DateRange? Period { get; private set; }
    public DateTime CreatedAt { get; private set; }

    internal static Result<InvoiceItem> Create(
        Guid id,
        Guid invoiceId,
        string description,
        int quantity,
        decimal unitPrice,
        DateTime? periodStart = null,
        DateTime? periodEnd = null)
    {
        var errors = new List<string>();

        DomainGuard.CheckNotEmpty(id, "InvoiceItem Id", errors);
        DomainGuard.CheckNotEmpty(invoiceId, nameof(InvoiceId), errors);
        string trimmedDescription = DomainGuard.CheckText(description, MaxDescriptionLength, nameof(Description), errors);
        DomainGuard.CheckPositive(quantity, nameof(Quantity), errors);
        Money? unitPriceVo = DomainGuard.Collect(Money.Create(unitPrice), errors);

        DateRange? period = null;
        if (periodStart.HasValue || periodEnd.HasValue)
        {
            if (!periodStart.HasValue || !periodEnd.HasValue)
            {
                errors.Add("Both PeriodStart and PeriodEnd must be specified together.");
            }
            else
            {
                period = DomainGuard.Collect(DateRange.Create(periodStart.Value, periodEnd.Value), errors);
            }
        }

        if (errors.Count != 0)
        {
            return DomainGuard.ValidationFailure<InvoiceItem>(errors);
        }

        return Result<InvoiceItem>.Success(new InvoiceItem(
            id, invoiceId,
            trimmedDescription, quantity, unitPriceVo!, period,
            createdAt: DateTime.UtcNow));
    }
}
