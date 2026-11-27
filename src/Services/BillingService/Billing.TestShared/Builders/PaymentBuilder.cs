using Billing.Domain.Entities;
using Billing.Domain.Enums;
using Billing.TestShared.Constants;
using BuildingBlocks.Domain.Results;

namespace Billing.TestShared.Builders;

public sealed class PaymentBuilder
{
    private Guid _id = BillingTestConstants.PaymentId;
    private Guid _customerId = BillingTestConstants.CustomerId;
    private Guid? _invoiceId = BillingTestConstants.InvoiceId;
    private Guid? _paymentMethodId = BillingTestConstants.PaymentMethodId;
    private decimal _amount = BillingTestConstants.PlanPrice;
    private string _currency = BillingTestConstants.Currency;
    private PaymentProvider _provider = PaymentProvider.Stripe;
    private string? _idempotencyKey = "idem-key-1";

    public PaymentBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public PaymentBuilder WithCustomerId(Guid customerId)
    {
        _customerId = customerId;
        return this;
    }

    public PaymentBuilder WithInvoiceId(Guid? invoiceId)
    {
        _invoiceId = invoiceId;
        return this;
    }

    public PaymentBuilder WithPaymentMethodId(Guid? paymentMethodId)
    {
        _paymentMethodId = paymentMethodId;
        return this;
    }

    public PaymentBuilder WithAmount(decimal amount, string currency = BillingTestConstants.Currency)
    {
        _amount = amount;
        _currency = currency;
        return this;
    }

    public PaymentBuilder WithProvider(PaymentProvider provider)
    {
        _provider = provider;
        return this;
    }

    public PaymentBuilder WithIdempotencyKey(string? idempotencyKey)
    {
        _idempotencyKey = idempotencyKey;
        return this;
    }

    public Payment Build()
    {
        Result<Payment> result = Payment.CreateCharge(
            _id,
            _customerId,
            _invoiceId,
            _paymentMethodId,
            _amount,
            _currency,
            _provider,
            _idempotencyKey);

        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Failed to build Payment: {result.Error.Message}");
        }

        return result.Value;
    }
}
