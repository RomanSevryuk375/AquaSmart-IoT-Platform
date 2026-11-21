using Billing.Domain.Entities;
using Billing.TestShared.Constants;
using BuildingBlocks.Domain.Results;

namespace Billing.TestShared.Builders;

public sealed class InvoiceBuilder
{
    private Guid _id = BillingTestConstants.InvoiceId;
    private Guid _customerId = BillingTestConstants.CustomerId;
    private Guid? _subscriptionId = BillingTestConstants.SubscriptionId;
    private string _number = BillingTestConstants.InvoiceNumberValue;
    private string _currency = BillingTestConstants.Currency;
    private DateTime _dueDate = DateTime.UtcNow.AddDays(14);
    private bool _withDefaultItem;

    public InvoiceBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public InvoiceBuilder WithCustomerId(Guid customerId)
    {
        _customerId = customerId;
        return this;
    }

    public InvoiceBuilder WithSubscriptionId(Guid? subscriptionId)
    {
        _subscriptionId = subscriptionId;
        return this;
    }

    public InvoiceBuilder WithNumber(string number)
    {
        _number = number;
        return this;
    }

    public InvoiceBuilder WithCurrency(string currency)
    {
        _currency = currency;
        return this;
    }

    public InvoiceBuilder WithDueDate(DateTime dueDate)
    {
        _dueDate = dueDate;
        return this;
    }

    public InvoiceBuilder WithDefaultItem()
    {
        _withDefaultItem = true;
        return this;
    }

    public Invoice Build()
    {
        Result<Invoice> result = Invoice.Create(
            _id,
            _customerId,
            _subscriptionId,
            _number,
            _currency,
            _dueDate);

        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Failed to build Invoice: {result.Error.Message}");
        }

        Invoice invoice = result.Value;
        if (_withDefaultItem)
        {
            invoice.AddItem(
                BillingTestConstants.InvoiceItemId,
                "Pro Monthly Subscription",
                1,
                BillingTestConstants.PlanPrice);
        }

        return invoice;
    }
}
