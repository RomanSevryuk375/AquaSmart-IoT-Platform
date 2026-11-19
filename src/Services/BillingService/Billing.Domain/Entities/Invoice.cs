using Billing.Domain.Enums;
using Billing.Domain.Events;
using Billing.Domain.ValueObjects;
using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Common;
using BuildingBlocks.Domain.Results;
using BuildingBlocks.Domain.ValueObjects;

namespace Billing.Domain.Entities;

public sealed class Invoice : AggregateRoot, IEntity
{
    private readonly List<InvoiceItem> _items = [];

    private Invoice(
        Guid id,
        Guid customerId,
        Guid? subscriptionId,
        InvoiceNumber number,
        string currency,
        DateTime dueDate,
        DateTime createdAt)
    {
        Id = id;
        CustomerId = customerId;
        SubscriptionId = subscriptionId;
        Number = number;
        Status = InvoiceStatus.Draft;
        Subtotal = Money.Parse(0m);
        Discount = Money.Parse(0m);
        Tax = Money.Parse(0m);
        Total = Money.Parse(0m);
        Currency = currency;
        DueDate = dueDate;
        CreatedAt = createdAt;
        IncrementVersion();
    }

#pragma warning disable CS8618
    private Invoice() { }
#pragma warning restore CS8618

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid? SubscriptionId { get; private set; }
    public InvoiceNumber Number { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public Money Subtotal { get; private set; }
    public Money Discount { get; private set; }
    public Money Tax { get; private set; }
    public Money Total { get; private set; }
    public string Currency { get; private set; }
    public DateTime DueDate { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyCollection<InvoiceItem> Items => _items.AsReadOnly();

    public static Result<Invoice> Create(
        Guid id,
        Guid customerId,
        Guid? subscriptionId,
        string number,
        string currency,
        DateTime dueDate)
    {
        var errors = new List<string>();

        DomainGuard.CheckNotEmpty(id, "Invoice Id", errors);
        DomainGuard.CheckNotEmpty(customerId, nameof(CustomerId), errors);

        InvoiceNumber? invoiceNumber = DomainGuard.Collect(InvoiceNumber.Create(number), errors);
        BillingMoney? currencyCheck = DomainGuard.Collect(BillingMoney.Zero(currency), errors);

        if (errors.Count != 0)
        {
            return DomainGuard.ValidationFailure<Invoice>(errors);
        }

        return Result<Invoice>.Success(new Invoice(
            id,
            customerId,
            subscriptionId,
            invoiceNumber!,
            currencyCheck!.Currency,
            dueDate,
            DateTime.UtcNow));
    }

    public Result<InvoiceItem> AddItem(
        Guid itemId,
        string description,
        int quantity,
        decimal unitPrice,
        DateTime? periodStart = null,
        DateTime? periodEnd = null)
    {
        Result editableCheck = EnsureModifiable();
        if (editableCheck.IsFailure)
        {
            return Result<InvoiceItem>.Failure(editableCheck.Error);
        }

        Result<InvoiceItem> itemResult = InvoiceItem.Create(
            itemId, invoiceId: Id,
            description, quantity, unitPrice,
            periodStart, periodEnd);
        if (itemResult.IsFailure)
        {
            return Result<InvoiceItem>.Failure(itemResult.Error);
        }

        _items.Add(itemResult.Value);
        RecalculateTotals();
        IncrementVersion();

        return Result<InvoiceItem>.Success(itemResult.Value);
    }

    public Result ApplyDiscount(decimal discountAmount)
    {
        Result editableCheck = EnsureModifiable();
        if (editableCheck.IsFailure)
        {
            return editableCheck;
        }

        Result<Money> discountResult = Money.Create(discountAmount);
        if (discountResult.IsFailure)
        {
            return Result.Failure(discountResult.Error);
        }

        if (discountResult.Value.Amount > Subtotal.Amount)
        {
            return Result.Failure(Error.Validation<Invoice>(
                "Discount cannot exceed invoice subtotal."));
        }

        Discount = discountResult.Value;
        RecalculateTotals();
        IncrementVersion();
        return Result.Success();
    }

    public Result ApplyTax(decimal taxAmount)
    {
        Result editableCheck = EnsureModifiable();
        if (editableCheck.IsFailure)
        {
            return editableCheck;
        }

        Result<Money> taxResult = Money.Create(taxAmount);
        if (taxResult.IsFailure)
        {
            return Result.Failure(taxResult.Error);
        }

        Tax = taxResult.Value;
        RecalculateTotals();
        IncrementVersion();
        return Result.Success();
    }

    public Result Finalize()
    {
        if (Status is not InvoiceStatus.Draft)
        {
            return Result.Failure(Error.Conflict<Invoice>(
                $"Only a Draft invoice can be finalized. Current status: {Status}."));
        }

        if (_items.Count == 0)
        {
            return Result.Failure(Error.Validation<Invoice>(
                "Cannot finalize an invoice without line items."));
        }

        Status = InvoiceStatus.Open;
        IncrementVersion();
        return Result.Success();
    }

    public Result MarkAsPaid(DateTime paidAt)
    {
        if (Status is not InvoiceStatus.Open)
        {
            return Result.Failure(Error.Conflict<Invoice>(
                $"Only an Open invoice can be marked as Paid. Current status: {Status}."));
        }

        Status = InvoiceStatus.Paid;
        PaidAt = paidAt;
        IncrementVersion();

        RaiseEvent(new InvoicePaidDomainEvent
        {
            CustomerId = CustomerId,
            InvoiceId = Id,
            InvoiceNumber = Number.Value,
            TotalAmount = Total.Amount,
            Currency = Currency,
            PaidAt = paidAt
        });

        return Result.Success();
    }

    public Result MarkAsVoid()
    {
        if (Status is not (InvoiceStatus.Draft or InvoiceStatus.Open))
        {
            return Result.Failure(Error.Conflict<Invoice>(
                $"Cannot void invoice in status {Status}."));
        }

        Status = InvoiceStatus.Void;
        IncrementVersion();
        return Result.Success();
    }

    public Result MarkAsUncollectible()
    {
        if (Status is not InvoiceStatus.Open)
        {
            return Result.Failure(Error.Conflict<Invoice>(
                $"Only an Open invoice can be marked as Uncollectible. Current status: {Status}."));
        }

        Status = InvoiceStatus.Uncollectible;
        IncrementVersion();
        return Result.Success();
    }

    private Result EnsureModifiable()
    {
        if (Status is InvoiceStatus.Paid or
                      InvoiceStatus.Void or
                      InvoiceStatus.Uncollectible)
        {
            return Result.Failure(Error.Conflict<Invoice>(
                $"Invoice cannot be modified in status {Status}."));
        }

        return Result.Success();
    }

    private void RecalculateTotals()
    {
        decimal subtotal = _items.Sum(i => i.TotalAmount.Amount);
        decimal total = subtotal - Discount.Amount + Tax.Amount;

        Subtotal = Money.Parse(subtotal);
        Total = Money.Parse(total < 0 ? 0m : total);
    }
}
