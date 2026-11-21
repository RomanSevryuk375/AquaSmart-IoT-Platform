namespace Billing.Domain.UnitTests.Entities;

public class PaymentTests
{
    [Fact]
    public void CreateCharge_WithEmptyId_ReturnsFailure()
    {
        // Act
        Result<Payment> result = Payment.CreateCharge(
            Guid.Empty,
            BillingTestConstants.CustomerId,
            BillingTestConstants.InvoiceId,
            BillingTestConstants.PaymentMethodId,
            50m,
            BillingTestConstants.Currency,
            PaymentProvider.Stripe);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Payment.Invalid");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void CreateCharge_WithNonPositiveAmount_ReturnsFailure(decimal amount)
    {
        // Act
        Result<Payment> result = Payment.CreateCharge(
            BillingTestConstants.PaymentId,
            BillingTestConstants.CustomerId,
            null,
            null,
            amount,
            BillingTestConstants.Currency,
            PaymentProvider.Stripe);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Payment.Invalid");
    }

    [Fact]
    public void CreateCharge_WithValidData_ReturnsPendingCharge()
    {
        // Arrange
        var id = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var tolerance = TimeSpan.FromSeconds(5);

        // Act
        Result<Payment> result = Payment.CreateCharge(
            id,
            customerId,
            BillingTestConstants.InvoiceId,
            BillingTestConstants.PaymentMethodId,
            49.99m,
            "usd",
            PaymentProvider.Stripe,
            "  idem-123  ");

        // Assert
        result.IsSuccess.Should().BeTrue();
        Payment payment = result.Value;
        payment.Id.Should().Be(id);
        payment.CustomerId.Should().Be(customerId);
        payment.Type.Should().Be(PaymentType.Charge);
        payment.Status.Should().Be(PaymentStatus.Pending);
        payment.Amount.Amount.Should().Be(49.99m);
        payment.Amount.Currency.Should().Be("USD");
        payment.RefundedAmount.Amount.Should().Be(0m);
        payment.IdempotencyKey.Should().Be("idem-123");
        payment.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, tolerance);
        payment.Version.Should().NotBeEmpty();
    }

    [Fact]
    public void MarkSucceeded_WhenPending_UpdatesStatusClearsErrorAndIncrementsVersion()
    {
        // Arrange
        Payment payment = new PaymentBuilder().Build();
        Guid initialVersion = payment.Version;

        // Act
        Result result = payment.MarkSucceeded("  ch_12345  ");

        // Assert
        result.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatus.Succeeded);
        payment.GatewayTransactionId.Should().Be("ch_12345");
        payment.ErrorCode.Should().BeNull();
        payment.ErrorMessage.Should().BeNull();
        payment.Version.Should().NotBe(initialVersion);
    }

    [Fact]
    public void MarkSucceeded_WhenNotPending_ReturnsConflict()
    {
        // Arrange
        Payment payment = new PaymentBuilder().Build();
        payment.MarkSucceeded(BillingTestConstants.GatewayTransactionId);
        Guid versionAfterSucceeded = payment.Version;

        // Act
        Result result = payment.MarkSucceeded("ch_other");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        payment.Version.Should().Be(versionAfterSucceeded);
    }

    [Fact]
    public void MarkFailed_WhenPending_SetsFailedStatusAndErrorDetails()
    {
        // Arrange
        Payment payment = new PaymentBuilder().Build();
        Guid initialVersion = payment.Version;

        // Act
        Result result = payment.MarkFailed("card_declined", "Insufficient funds.");

        // Assert
        result.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatus.Failed);
        payment.ErrorCode.Should().Be("card_declined");
        payment.ErrorMessage.Should().Be("Insufficient funds.");
        payment.Version.Should().NotBe(initialVersion);
    }

    [Fact]
    public void CreateRefund_ForSucceededCharge_CreatesRefundAndUpdatesOriginalChargeStatus()
    {
        // Arrange
        Payment charge = new PaymentBuilder().WithAmount(100m).Build();
        charge.MarkSucceeded(BillingTestConstants.GatewayTransactionId);
        Guid initialChargeVersion = charge.Version;

        // Act
        Result<Payment> partialRefund = Payment.CreateRefund(Guid.NewGuid(), charge, 40m);
        Result<Payment> finalRefund = Payment.CreateRefund(Guid.NewGuid(), charge, 60m);

        // Assert
        partialRefund.IsSuccess.Should().BeTrue();
        partialRefund.Value.Type.Should().Be(PaymentType.Refund);
        partialRefund.Value.OriginalPaymentId.Should().Be(charge.Id);

        finalRefund.IsSuccess.Should().BeTrue();
        charge.RefundedAmount.Amount.Should().Be(100m);
        charge.Status.Should().Be(PaymentStatus.Refunded);
        charge.Version.Should().NotBe(initialChargeVersion);
    }

    [Fact]
    public void CreateRefund_ExceedingOriginalAmount_ReturnsFailureAndDoesNotMutate()
    {
        // Arrange
        Payment charge = new PaymentBuilder().WithAmount(50m).Build();
        charge.MarkSucceeded(BillingTestConstants.GatewayTransactionId);
        Guid versionBefore = charge.Version;

        // Act
        Result<Payment> result = Payment.CreateRefund(Guid.NewGuid(), charge, 60m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Payment.Invalid");
        charge.RefundedAmount.Amount.Should().Be(0m);
        charge.Status.Should().Be(PaymentStatus.Succeeded);
        charge.Version.Should().Be(versionBefore);
    }

    [Fact]
    public void CreateRefund_ForPendingCharge_ReturnsConflict()
    {
        // Arrange
        Payment charge = new PaymentBuilder().WithAmount(50m).Build();

        // Act
        Result<Payment> result = Payment.CreateRefund(Guid.NewGuid(), charge, 25m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
    }
}
