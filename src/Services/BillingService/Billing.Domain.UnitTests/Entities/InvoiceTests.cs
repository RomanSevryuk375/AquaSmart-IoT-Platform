namespace Billing.Domain.UnitTests.Entities;

public class InvoiceTests
{
    [Fact]
    public void Create_WithEmptyId_ReturnsFailure()
    {
        // Act
        Result<Invoice> result = Invoice.Create(
            Guid.Empty,
            BillingTestConstants.CustomerId,
            BillingTestConstants.SubscriptionId,
            BillingTestConstants.InvoiceNumberValue,
            BillingTestConstants.Currency,
            DateTime.UtcNow.AddDays(7));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Invoice.Invalid");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Create_WithInvalidInvoiceNumber_ReturnsFailure()
    {
        // Act
        Result<Invoice> result = Invoice.Create(
            BillingTestConstants.InvoiceId,
            BillingTestConstants.CustomerId,
            null,
            "BAD-NUM",
            BillingTestConstants.Currency,
            DateTime.UtcNow.AddDays(7));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Invoice.Invalid");
    }

    [Fact]
    public void Create_WithValidData_ReturnsDraftInvoiceWithZeroTotals()
    {
        // Arrange
        var id = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var dueDate = DateTime.UtcNow.AddDays(10);
        var tolerance = TimeSpan.FromSeconds(5);

        // Act
        Result<Invoice> result = Invoice.Create(
            id,
            customerId,
            null,
            "inv-2026-00010",
            "eur",
            dueDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        Invoice invoice = result.Value;
        invoice.Id.Should().Be(id);
        invoice.CustomerId.Should().Be(customerId);
        invoice.Number.Value.Should().Be("INV-2026-00010");
        invoice.Currency.Should().Be("EUR");
        invoice.Status.Should().Be(InvoiceStatus.Draft);
        invoice.Subtotal.Amount.Should().Be(0m);
        invoice.Total.Amount.Should().Be(0m);
        invoice.Items.Should().BeEmpty();
        invoice.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, tolerance);
        invoice.Version.Should().NotBeEmpty();
    }

    [Fact]
    public void AddItem_WithValidData_RecalculatesTotalsAndIncrementsVersion()
    {
        // Arrange
        Invoice invoice = new InvoiceBuilder().Build();
        Guid initialVersion = invoice.Version;

        // Act
        Result<InvoiceItem> result = invoice.AddItem(
            Guid.NewGuid(),
            "Pro Plan (x2)",
            2,
            15.50m);

        // Assert
        result.IsSuccess.Should().BeTrue();
        invoice.Items.Should().ContainSingle();
        invoice.Subtotal.Amount.Should().Be(31.00m);
        invoice.Total.Amount.Should().Be(31.00m);
        invoice.Version.Should().NotBe(initialVersion);
    }

    [Fact]
    public void ApplyDiscount_ExceedingSubtotal_ReturnsFailureAndDoesNotMutate()
    {
        // Arrange
        Invoice invoice = new InvoiceBuilder().WithDefaultItem().Build();
        Guid initialVersion = invoice.Version;
        decimal initialTotal = invoice.Total.Amount;

        // Act
        Result result = invoice.ApplyDiscount(initialTotal + 10m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Invoice.Invalid");
        invoice.Total.Amount.Should().Be(initialTotal);
        invoice.Version.Should().Be(initialVersion);
    }

    [Fact]
    public void ApplyDiscountAndTax_WithValidAmounts_RecalculatesTotal()
    {
        // Arrange
        Invoice invoice = new InvoiceBuilder().Build();
        invoice.AddItem(Guid.NewGuid(), "Item", 1, 100m);

        // Act
        Result discountResult = invoice.ApplyDiscount(20m);
        Result taxResult = invoice.ApplyTax(8m);

        // Assert
        discountResult.IsSuccess.Should().BeTrue();
        taxResult.IsSuccess.Should().BeTrue();
        invoice.Subtotal.Amount.Should().Be(100m);
        invoice.Discount.Amount.Should().Be(20m);
        invoice.Tax.Amount.Should().Be(8m);
        invoice.Total.Amount.Should().Be(88m);
    }

    [Fact]
    public void Finalize_WithoutItems_ReturnsFailure()
    {
        // Arrange
        Invoice invoice = new InvoiceBuilder().Build();
        Guid initialVersion = invoice.Version;

        // Act
        Result result = invoice.Finalize();

        // Assert
        result.IsFailure.Should().BeTrue();
        invoice.Status.Should().Be(InvoiceStatus.Draft);
        invoice.Version.Should().Be(initialVersion);
    }

    [Fact]
    public void MarkAsPaid_WhenOpen_SetsPaidRaisesDomainEventAndIncrementsVersion()
    {
        // Arrange
        Invoice invoice = new InvoiceBuilder().WithDefaultItem().Build();
        invoice.Finalize();
        Guid initialVersion = invoice.Version;
        DateTime paidAt = DateTime.UtcNow;

        // Act
        Result result = invoice.MarkAsPaid(paidAt);

        // Assert
        result.IsSuccess.Should().BeTrue();
        invoice.Status.Should().Be(InvoiceStatus.Paid);
        invoice.PaidAt.Should().Be(paidAt);
        invoice.Version.Should().NotBe(initialVersion);

        invoice.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<InvoicePaidDomainEvent>();

        InvoicePaidDomainEvent paidEvent = invoice.DomainEvents.OfType<InvoicePaidDomainEvent>().Single();
        paidEvent.InvoiceId.Should().Be(invoice.Id);
        paidEvent.CustomerId.Should().Be(invoice.CustomerId);
        paidEvent.InvoiceNumber.Should().Be(invoice.Number.Value);
        paidEvent.TotalAmount.Should().Be(invoice.Total.Amount);
        paidEvent.Currency.Should().Be(invoice.Currency);
        paidEvent.PaidAt.Should().Be(paidAt);
    }

    [Fact]
    public void AddItem_WhenPaid_ReturnsConflictAndDoesNotMutate()
    {
        // Arrange
        Invoice invoice = new InvoiceBuilder().WithDefaultItem().Build();
        invoice.Finalize();
        invoice.MarkAsPaid(DateTime.UtcNow);
        Guid versionAfterPaid = invoice.Version;

        // Act
        Result<InvoiceItem> result = invoice.AddItem(Guid.NewGuid(), "Extra", 1, 10m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        invoice.Items.Should().ContainSingle();
        invoice.Version.Should().Be(versionAfterPaid);
    }
}
