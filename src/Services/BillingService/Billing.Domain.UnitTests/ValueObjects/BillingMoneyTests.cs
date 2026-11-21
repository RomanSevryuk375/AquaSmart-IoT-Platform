namespace Billing.Domain.UnitTests.ValueObjects;

public class BillingMoneyTests
{
    [Fact]
    public void Create_WithNegativeAmount_ReturnsFailure()
    {
        // Act
        Result<BillingMoney> result = BillingMoney.Create(-1m, "USD");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("BillingMoney.Invalid");
        result.Error.Message.Should().Be("Amount cannot be negative.");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespaceCurrency_ReturnsFailure(string? currency)
    {
        // Act
        Result<BillingMoney> result = BillingMoney.Create(10m, currency!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("BillingMoney.Invalid");
        result.Error.Message.Should().Be("Currency is required.");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData("US")]
    [InlineData("USDD")]
    [InlineData("123")]
    [InlineData("U$D")]
    public void Create_WithInvalidCurrencyCode_ReturnsFailure(string invalidCurrency)
    {
        // Act
        Result<BillingMoney> result = BillingMoney.Create(10m, invalidCurrency);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("BillingMoney.Invalid");
        result.Error.Message.Should().Be("Currency must be a valid 3-letter ISO 4217 code.");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData(0, "usd", "0.00 USD", "USD")]
    [InlineData(19.99, "  eur  ", "19.99 EUR", "EUR")]
    public void Create_WithValidData_ReturnsSuccessAndNormalizesCurrency(
        decimal amount,
        string currency,
        string expectedString,
        string expectedCurrency)
    {
        // Act
        Result<BillingMoney> result = BillingMoney.Create(amount, currency);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(amount);
        result.Value.Currency.Should().Be(expectedCurrency);
        result.Value.ToString().Should().Be(expectedString);
    }

    [Fact]
    public void Add_WithSameCurrency_ReturnsSum()
    {
        // Arrange
        BillingMoney left = BillingMoney.Create(10.50m, "USD").Value;
        BillingMoney right = BillingMoney.Create(4.50m, "USD").Value;

        // Act
        Result<BillingMoney> result = left.Add(right);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(15.00m);
        result.Value.Currency.Should().Be("USD");
    }

    [Fact]
    public void Add_WithDifferentCurrencies_ReturnsFailure()
    {
        // Arrange
        BillingMoney usd = BillingMoney.Create(10m, "USD").Value;
        BillingMoney eur = BillingMoney.Create(5m, "EUR").Value;

        // Act
        Result<BillingMoney> result = usd.Add(eur);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("BillingMoney.Invalid");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Subtract_WithResultingNegativeAmount_ReturnsFailure()
    {
        // Arrange
        BillingMoney smaller = BillingMoney.Create(5m, "USD").Value;
        BillingMoney larger = BillingMoney.Create(10m, "USD").Value;

        // Act
        Result<BillingMoney> result = smaller.Subtract(larger);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("BillingMoney.Invalid");
        result.Error.Message.Should().Be("Resulting amount cannot be negative.");
    }

    [Fact]
    public void Subtract_WithDifferentCurrencies_ReturnsFailure()
    {
        // Arrange
        BillingMoney usd = BillingMoney.Create(10m, "USD").Value;
        BillingMoney eur = BillingMoney.Create(5m, "EUR").Value;

        // Act
        Result<BillingMoney> result = usd.Subtract(eur);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("BillingMoney.Invalid");
    }

    [Fact]
    public void Equality_WithSameAmountAndCurrency_ReturnsTrue()
    {
        // Arrange
        BillingMoney first = BillingMoney.Create(25m, "usd").Value;
        BillingMoney second = BillingMoney.Create(25m, "USD").Value;
        BillingMoney different = BillingMoney.Create(30m, "USD").Value;

        // Assert
        first.Should().Be(second);
        (first == second).Should().BeTrue();
        first.Should().NotBe(different);
    }
}
