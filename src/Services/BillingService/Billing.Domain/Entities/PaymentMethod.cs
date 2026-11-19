using Billing.Domain.Enums;
using Billing.Domain.ValueObjects;
using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Common;
using BuildingBlocks.Domain.Results;

namespace Billing.Domain.Entities;

public sealed class PaymentMethod : IEntity, IHasVersion
{
    public const int MaxGatewayTokenLength = 256;

    private PaymentMethod(
        Guid id,
        Guid customerId,
        PaymentMethodType type,
        PaymentProvider provider,
        string gatewayToken,
        CardDetails? cardDetails,
        DateTime createdAt)
    {
        Id = id;
        CustomerId = customerId;
        Type = type;
        Provider = provider;
        GatewayToken = gatewayToken;
        CardDetails = cardDetails;
        IsActive = true;
        CreatedAt = createdAt;
        Version = Guid.NewGuid();
    }

#pragma warning disable CS8618
    private PaymentMethod() { }
#pragma warning restore CS8618

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public PaymentMethodType Type { get; private set; }
    public PaymentProvider Provider { get; private set; }
    public string GatewayToken { get; private set; }
    public CardDetails? CardDetails { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid Version { get; private set; }

    internal static Result<PaymentMethod> Create(
        Guid id,
        Guid customerId,
        PaymentMethodType type,
        PaymentProvider provider,
        string gatewayToken,
        string? cardLast4 = null,
        string? cardBrand = null,
        int? expMonth = null,
        int? expYear = null)
    {
        var errors = new List<string>();

        DomainGuard.CheckNotEmpty(id, "PaymentMethod Id", errors);
        DomainGuard.CheckNotEmpty(customerId, nameof(CustomerId), errors);
        DomainGuard.CheckEnum(type, "payment method type", errors);
        DomainGuard.CheckEnum(provider, "payment provider", errors);
        string cleanToken = DomainGuard.CheckText(gatewayToken, MaxGatewayTokenLength, "Gateway token", errors);

        CardDetails? cardDetails = null;
        bool hasAnyCardField = cardLast4 is not null ||
                               cardBrand is not null ||
                               expMonth.HasValue ||
                               expYear.HasValue;

        if (type == PaymentMethodType.CreditCard || hasAnyCardField)
        {
            if (cardLast4 is null ||
                cardBrand is null ||
                !expMonth.HasValue ||
                !expYear.HasValue)
            {
                errors.Add("Card details (last4, brand, expMonth, expYear) are required.");
            }
            else
            {
                cardDetails = DomainGuard.Collect(
                    CardDetails.Create(cardLast4, cardBrand, expMonth.Value, expYear.Value),
                    errors);
            }
        }

        if (errors.Count != 0)
        {
            return DomainGuard.ValidationFailure<PaymentMethod>(errors);
        }

        return Result<PaymentMethod>.Success(new PaymentMethod(
            id, customerId,
            type, provider, cleanToken, cardDetails,
            createdAt: DateTime.UtcNow));
    }

    internal void Deactivate()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        Version = Guid.NewGuid();
    }
}
