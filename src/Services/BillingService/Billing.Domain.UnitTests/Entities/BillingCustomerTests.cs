namespace Billing.Domain.UnitTests.Entities;

public class BillingCustomerTests
{
    [Fact]
    public void Create_WithEmptyId_ReturnsFailure()
    {
        // Act
        Result<BillingCustomer> result = BillingCustomer.Create(
            Guid.Empty,
            BillingTestConstants.UserId,
            BillingTestConstants.CustomerEmail);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("BillingCustomer.Invalid");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Create_WithEmptyUserId_ReturnsFailure()
    {
        // Act
        Result<BillingCustomer> result = BillingCustomer.Create(
            BillingTestConstants.CustomerId,
            Guid.Empty,
            BillingTestConstants.CustomerEmail);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("BillingCustomer.Invalid");
    }

    [Fact]
    public void Create_WithInvalidEmail_ReturnsFailure()
    {
        // Act
        Result<BillingCustomer> result = BillingCustomer.Create(
            BillingTestConstants.CustomerId,
            BillingTestConstants.UserId,
            "not-an-email");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("BillingCustomer.Invalid");
    }

    [Fact]
    public void Create_WithValidData_ReturnsSuccessAndInitializesProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var tolerance = TimeSpan.FromSeconds(5);

        // Act
        Result<BillingCustomer> result = BillingCustomer.Create(id, userId, "  Customer@Example.Com  ");

        // Assert
        result.IsSuccess.Should().BeTrue();
        BillingCustomer customer = result.Value;
        customer.Id.Should().Be(id);
        customer.UserId.Should().Be(userId);
        customer.Email.Value.Should().Be("customer@example.com");
        customer.GatewayCustomerId.Should().BeNull();
        customer.DefaultPaymentMethodId.Should().BeNull();
        customer.PaymentMethods.Should().BeEmpty();
        customer.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, tolerance);
        customer.Version.Should().NotBeEmpty();
    }

    [Fact]
    public void AddPaymentMethod_WithValidCard_AddsMethodSetsDefaultAndIncrementsVersion()
    {
        // Arrange
        BillingCustomer customer = new BillingCustomerBuilder().Build();
        Guid initialVersion = customer.Version;
        var pmId = Guid.NewGuid();

        // Act
        Result<PaymentMethod> result = customer.AddPaymentMethod(
            pmId,
            PaymentMethodType.CreditCard,
            PaymentProvider.Stripe,
            BillingTestConstants.GatewayToken,
            "4242",
            "Visa",
            12,
            2030);

        // Assert
        result.IsSuccess.Should().BeTrue();
        customer.PaymentMethods.Should().ContainSingle();
        customer.DefaultPaymentMethodId.Should().Be(pmId);
        customer.Version.Should().NotBe(initialVersion);
        result.Value.CardDetails.Should().NotBeNull();
        result.Value.CardDetails!.CardLast4.Should().Be("4242");
    }

    [Fact]
    public void AddPaymentMethod_WithDuplicateActiveToken_ReturnsConflictAndDoesNotMutate()
    {
        // Arrange
        BillingCustomer customer = new BillingCustomerBuilder().Build();
        customer.AddPaymentMethod(
            Guid.NewGuid(),
            PaymentMethodType.CreditCard,
            PaymentProvider.Stripe,
            BillingTestConstants.GatewayToken,
            "4242",
            "Visa",
            12,
            2030);
        Guid versionAfterFirst = customer.Version;

        // Act
        Result<PaymentMethod> result = customer.AddPaymentMethod(
            Guid.NewGuid(),
            PaymentMethodType.CreditCard,
            PaymentProvider.Stripe,
            BillingTestConstants.GatewayToken,
            "4242",
            "Visa",
            12,
            2030);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        customer.PaymentMethods.Should().ContainSingle();
        customer.Version.Should().Be(versionAfterFirst);
    }

    [Fact]
    public void SetDefaultPaymentMethod_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        BillingCustomer customer = new BillingCustomerBuilder().Build();
        Guid initialVersion = customer.Version;

        // Act
        Result result = customer.SetDefaultPaymentMethod(Guid.NewGuid());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
        customer.Version.Should().Be(initialVersion);
    }

    [Fact]
    public void RemovePaymentMethod_WhenDefault_DeactivatesAndReassignsDefault()
    {
        // Arrange
        BillingCustomer customer = new BillingCustomerBuilder().Build();
        var pm1 = Guid.NewGuid();
        var pm2 = Guid.NewGuid();
        customer.AddPaymentMethod(pm1, PaymentMethodType.Sbp, PaymentProvider.Stripe, "tok_1", setAsDefault: true);
        customer.AddPaymentMethod(pm2, PaymentMethodType.PayPal, PaymentProvider.Stripe, "tok_2", setAsDefault: false);
        Guid initialVersion = customer.Version;

        // Act
        Result result = customer.RemovePaymentMethod(pm1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        customer.PaymentMethods.Single(pm => pm.Id == pm1).IsActive.Should().BeFalse();
        customer.DefaultPaymentMethodId.Should().Be(pm2);
        customer.Version.Should().NotBe(initialVersion);
    }

    [Fact]
    public void UpdateEmail_WithSameEmail_DoesNotIncrementVersion()
    {
        // Arrange
        BillingCustomer customer = new BillingCustomerBuilder()
            .WithEmail("user@example.com")
            .Build();
        Guid initialVersion = customer.Version;

        // Act
        Result result = customer.UpdateEmail("USER@example.com");

        // Assert
        result.IsSuccess.Should().BeTrue();
        customer.Version.Should().Be(initialVersion);
    }

    [Fact]
    public void UpdateEmail_WithInvalidEmail_ReturnsFailureAndDoesNotMutate()
    {
        // Arrange
        BillingCustomer customer = new BillingCustomerBuilder().Build();
        string initialEmail = customer.Email.Value;
        Guid initialVersion = customer.Version;

        // Act
        Result result = customer.UpdateEmail("bad-email");

        // Assert
        result.IsFailure.Should().BeTrue();
        customer.Email.Value.Should().Be(initialEmail);
        customer.Version.Should().Be(initialVersion);
    }

    [Fact]
    public void UpdateGatewayCustomerId_WithValidId_UpdatesAndIncrementsVersion()
    {
        // Arrange
        BillingCustomer customer = new BillingCustomerBuilder().Build();
        Guid initialVersion = customer.Version;

        // Act
        Result result = customer.UpdateGatewayCustomerId("  cus_12345  ");

        // Assert
        result.IsSuccess.Should().BeTrue();
        customer.GatewayCustomerId.Should().Be("cus_12345");
        customer.Version.Should().NotBe(initialVersion);
    }
}
