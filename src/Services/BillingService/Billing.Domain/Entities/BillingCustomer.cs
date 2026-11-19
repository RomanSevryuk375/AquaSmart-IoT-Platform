using Billing.Domain.Enums;
using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Common;
using BuildingBlocks.Domain.Results;
using IdentityService.Domain.ValueObjects;

namespace Billing.Domain.Entities;

public sealed class BillingCustomer : AggregateRoot, IEntity
{
    public const int MaxGatewayCustomerIdLength = 128;

    private readonly List<PaymentMethod> _paymentMethods = [];

    private BillingCustomer(
        Guid id,
        Guid userId,
        EmailAddress email,
        DateTime createdAt)
    {
        Id = id;
        UserId = userId;
        Email = email;
        CreatedAt = createdAt;
    }

#pragma warning disable CS8618
    private BillingCustomer() { }
#pragma warning restore CS8618

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public EmailAddress Email { get; private set; }
    public string? GatewayCustomerId { get; private set; }
    public Guid? DefaultPaymentMethodId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyCollection<PaymentMethod> PaymentMethods => _paymentMethods.AsReadOnly();

    public static Result<BillingCustomer> Create(
        Guid id,
        Guid userId,
        string email)
    {
        var errors = new List<string>();

        DomainGuard.CheckNotEmpty(id, "BillingCustomer Id", errors);
        DomainGuard.CheckNotEmpty(userId, nameof(UserId), errors);
        EmailAddress? emailVo = DomainGuard.Collect(EmailAddress.Create(email), errors);

        if (errors.Count != 0)
        {
            return DomainGuard.ValidationFailure<BillingCustomer>(errors);
        }

        return Result<BillingCustomer>.Success(new BillingCustomer(
            id,
            userId,
            emailVo!,
            DateTime.UtcNow));
    }

    public Result<PaymentMethod> AddPaymentMethod(
        Guid paymentMethodId,
        PaymentMethodType type,
        PaymentProvider provider,
        string gatewayToken,
        string? cardLast4 = null,
        string? cardBrand = null,
        int? expMonth = null,
        int? expYear = null,
        bool setAsDefault = false)
    {
        Result<PaymentMethod> methodResult = PaymentMethod.Create(
            paymentMethodId, customerId: Id,
            type, provider, gatewayToken,
            cardLast4, cardBrand, expMonth, expYear);
        if (methodResult.IsFailure)
        {
            return Result<PaymentMethod>.Failure(methodResult.Error);
        }

        PaymentMethod method = methodResult.Value;

        bool duplicateExists = _paymentMethods.Exists(pm =>
            pm.IsActive &&
            pm.Provider == method.Provider &&
            string.Equals(pm.GatewayToken, method.GatewayToken, StringComparison.Ordinal));

        if (duplicateExists)
        {
            return Result<PaymentMethod>.Failure(Error.Conflict<PaymentMethod>(
                "An active payment method with this gateway token already exists."));
        }

        _paymentMethods.Add(method);

        if (setAsDefault || DefaultPaymentMethodId is null)
        {
            DefaultPaymentMethodId = method.Id;
        }

        IncrementVersion();
        return Result<PaymentMethod>.Success(method);
    }

    public Result SetDefaultPaymentMethod(Guid paymentMethodId)
    {
        PaymentMethod? method = _paymentMethods.Find(pm => pm.Id == paymentMethodId && pm.IsActive);
        if (method is null)
        {
            return Result.Failure(Error.NotFound<PaymentMethod>(
                "Active payment method was not found for this customer."));
        }

        if (DefaultPaymentMethodId == paymentMethodId)
        {
            return Result.Success();
        }

        DefaultPaymentMethodId = paymentMethodId;
        IncrementVersion();
        return Result.Success();
    }

    public Result RemovePaymentMethod(Guid paymentMethodId)
    {
        PaymentMethod? method = _paymentMethods.Find(pm => pm.Id == paymentMethodId && pm.IsActive);
        if (method is null)
        {
            return Result.Failure(Error.NotFound<PaymentMethod>(
                "Active payment method was not found for this customer."));
        }

        method.Deactivate();

        if (DefaultPaymentMethodId == paymentMethodId)
        {
            PaymentMethod? nextActive = _paymentMethods
                .Where(pm => pm.IsActive && pm.Id != paymentMethodId)
                .OrderByDescending(pm => pm.CreatedAt)
                .FirstOrDefault();

            DefaultPaymentMethodId = nextActive?.Id;
        }

        IncrementVersion();
        return Result.Success();
    }

    public Result UpdateEmail(string email)
    {
        Result<EmailAddress> emailResult = EmailAddress.Create(email);
        if (emailResult.IsFailure)
        {
            return Result.Failure(emailResult.Error);
        }

        if (Email == emailResult.Value)
        {
            return Result.Success();
        }

        Email = emailResult.Value;
        IncrementVersion();
        return Result.Success();
    }

    public Result UpdateGatewayCustomerId(string gatewayCustomerId)
    {
        Result<string> gatewayIdResult = DomainGuard.RequireText<BillingCustomer>(
            gatewayCustomerId, MaxGatewayCustomerIdLength, nameof(GatewayCustomerId));
        if (gatewayIdResult.IsFailure)
        {
            return Result.Failure(gatewayIdResult.Error);
        }

        if (string.Equals(GatewayCustomerId, gatewayIdResult.Value, StringComparison.Ordinal))
        {
            return Result.Success();
        }

        GatewayCustomerId = gatewayIdResult.Value;
        IncrementVersion();
        return Result.Success();
    }
}
