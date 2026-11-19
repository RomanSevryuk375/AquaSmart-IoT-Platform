using Billing.Domain.Enums;
using Billing.Domain.ValueObjects;
using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Common;
using BuildingBlocks.Domain.Results;
using BuildingBlocks.Domain.ValueObjects;

namespace Billing.Domain.Entities;

public sealed class Payment : AggregateRoot, IEntity
{
    public const int MaxGatewayTransactionIdLength = 256;
    public const int MaxIdempotencyKeyLength = 128;
    public const int MaxErrorCodeLength = 64;
    public const int MaxErrorMessageLength = 512;

    private Payment(
        Guid id,
        Guid customerId,
        Guid? invoiceId,
        Guid? paymentMethodId,
        Guid? originalPaymentId,
        PaymentType type,
        PaymentStatus status,
        BillingMoney amount,
        PaymentProvider provider,
        string? idempotencyKey,
        DateTime createdAt)
    {
        Id = id;
        CustomerId = customerId;
        InvoiceId = invoiceId;
        PaymentMethodId = paymentMethodId;
        OriginalPaymentId = originalPaymentId;
        Type = type;
        Status = status;
        Amount = amount;
        RefundedAmount = Money.Parse(0m);
        Provider = provider;
        IdempotencyKey = idempotencyKey;
        CreatedAt = createdAt;
    }

#pragma warning disable CS8618
    private Payment() { }
#pragma warning restore CS8618

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid? InvoiceId { get; private set; }
    public Guid? PaymentMethodId { get; private set; }
    public Guid? OriginalPaymentId { get; private set; }
    public PaymentType Type { get; private set; }
    public PaymentStatus Status { get; private set; }
    public BillingMoney Amount { get; private set; }
    public Money RefundedAmount { get; private set; }
    public PaymentProvider Provider { get; private set; }
    public string? GatewayTransactionId { get; private set; }
    public string? IdempotencyKey { get; private set; }
    public string? ErrorCode { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static Result<Payment> CreateCharge(
        Guid id,
        Guid customerId,
        Guid? invoiceId,
        Guid? paymentMethodId,
        decimal amount,
        string currency,
        PaymentProvider provider,
        string? idempotencyKey = null)
    {
        var errors = new List<string>();

        DomainGuard.CheckNotEmpty(id, "Payment Id", errors);
        DomainGuard.CheckNotEmpty(customerId, nameof(CustomerId), errors);
        DomainGuard.CheckPositive(amount, "Charge amount", errors);

        BillingMoney? money = DomainGuard.Collect(BillingMoney.Create(amount, currency), errors);
        DomainGuard.CheckEnum(provider, "payment provider", errors);
        string? cleanKey = DomainGuard.CheckOptionalText(
            idempotencyKey, MaxIdempotencyKeyLength, nameof(IdempotencyKey), errors);

        if (errors.Count != 0)
        {
            return DomainGuard.ValidationFailure<Payment>(errors);
        }

        return Result<Payment>.Success(new Payment(
            id, customerId, invoiceId, paymentMethodId, originalPaymentId: null,
            PaymentType.Charge, PaymentStatus.Pending,
            money!, provider, cleanKey,
            createdAt: DateTime.UtcNow));
    }

    public static Result<Payment> CreateRefund(
        Guid id,
        Payment originalCharge,
        decimal refundAmount,
        string? idempotencyKey = null)
    {
        var errors = new List<string>();

        DomainGuard.CheckNotEmpty(id, "Refund Payment Id", errors);

        if (originalCharge is null)
        {
            errors.Add("Original charge payment is required.");
            return DomainGuard.ValidationFailure<Payment>(errors);
        }

        if (originalCharge.Type is not PaymentType.Charge)
        {
            errors.Add("Can only refund a Charge payment.");
        }

        if (originalCharge.Status is not PaymentStatus.Succeeded)
        {
            return Result<Payment>.Failure(Error.Conflict<Payment>(
                $"Cannot refund payment in status {originalCharge.Status}."));
        }

        DomainGuard.CheckPositive(refundAmount, "Refund amount", errors);
        if (refundAmount > 0 &&
            originalCharge.RefundedAmount.Amount + refundAmount > originalCharge.Amount.Amount)
        {
            errors.Add("Total refunded amount cannot exceed original charge amount.");
        }

        string? cleanKey = DomainGuard.CheckOptionalText(
            idempotencyKey, MaxIdempotencyKeyLength, nameof(IdempotencyKey), errors);
        BillingMoney? refundMoney = DomainGuard.Collect(
            BillingMoney.Create(refundAmount, originalCharge.Amount.Currency),
            errors);

        if (errors.Count != 0)
        {
            return DomainGuard.ValidationFailure<Payment>(errors);
        }

        originalCharge.RegisterRefund(refundAmount);

        return Result<Payment>.Success(new Payment(
            id,
            originalCharge.CustomerId,
            originalCharge.InvoiceId,
            originalCharge.PaymentMethodId,
            originalCharge.Id,
            PaymentType.Refund,
            PaymentStatus.Pending,
            refundMoney!,
            originalCharge.Provider,
            cleanKey,
            DateTime.UtcNow));
    }

    public Result MarkSucceeded(string gatewayTransactionId)
    {
        if (Status is not PaymentStatus.Pending)
        {
            return Result.Failure(Error.Conflict<Payment>(
                $"Only a Pending payment can be marked as Succeeded. Current status: {Status}."));
        }

        Result<string> txResult = DomainGuard.RequireText<Payment>(
            gatewayTransactionId, MaxGatewayTransactionIdLength, nameof(GatewayTransactionId));
        if (txResult.IsFailure)
        {
            return Result.Failure(txResult.Error);
        }

        Status = PaymentStatus.Succeeded;
        GatewayTransactionId = txResult.Value;
        ErrorCode = null;
        ErrorMessage = null;
        IncrementVersion();

        return Result.Success();
    }

    public Result MarkFailed(string errorCode, string errorMessage)
    {
        if (Status is not PaymentStatus.Pending)
        {
            return Result.Failure(Error.Conflict<Payment>(
                $"Only a Pending payment can be marked as Failed. Current status: {Status}."));
        }

        Result<string> codeResult = DomainGuard.RequireText<Payment>(
            errorCode, MaxErrorCodeLength, nameof(ErrorCode));
        if (codeResult.IsFailure)
        {
            return Result.Failure(codeResult.Error);
        }

        Result<string> msgResult = DomainGuard.RequireText<Payment>(
            errorMessage, MaxErrorMessageLength, nameof(ErrorMessage));
        if (msgResult.IsFailure)
        {
            return Result.Failure(msgResult.Error);
        }

        Status = PaymentStatus.Failed;
        ErrorCode = codeResult.Value;
        ErrorMessage = msgResult.Value;
        IncrementVersion();

        return Result.Success();
    }

    public Result MarkCanceled()
    {
        if (Status is not PaymentStatus.Pending)
        {
            return Result.Failure(Error.Conflict<Payment>(
                $"Only a Pending payment can be marked as Canceled. Current status: {Status}."));
        }

        Status = PaymentStatus.Canceled;
        IncrementVersion();
        return Result.Success();
    }

    private void RegisterRefund(decimal refundAmount)
    {
        RefundedAmount = Money.Parse(RefundedAmount.Amount + refundAmount);
        if (RefundedAmount.Amount >= Amount.Amount)
        {
            Status = PaymentStatus.Refunded;
        }

        IncrementVersion();
    }
}
