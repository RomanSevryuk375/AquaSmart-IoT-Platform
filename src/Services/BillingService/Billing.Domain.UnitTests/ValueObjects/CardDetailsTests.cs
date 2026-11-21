namespace Billing.Domain.UnitTests.ValueObjects;

public class CardDetailsTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("12345")]
    [InlineData("12ab")]
    public void Create_WithInvalidLast4_ReturnsFailure(string? last4)
    {
        // Act
        Result<CardDetails> result = CardDetails.Create(last4!, "Visa", 12, 2030);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CardDetails.Invalid");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyBrand_ReturnsFailure(string? brand)
    {
        // Act
        Result<CardDetails> result = CardDetails.Create("4242", brand!, 12, 2030);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CardDetails.Invalid");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void Create_WithInvalidExpMonth_ReturnsFailure(int month)
    {
        // Act
        Result<CardDetails> result = CardDetails.Create("4242", "Visa", month, 2030);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CardDetails.Invalid");
    }

    [Theory]
    [InlineData(1999)]
    [InlineData(2101)]
    public void Create_WithInvalidExpYear_ReturnsFailure(int year)
    {
        // Act
        Result<CardDetails> result = CardDetails.Create("4242", "Visa", 12, year);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CardDetails.Invalid");
    }

    [Fact]
    public void Create_WithValidData_ReturnsSuccessAndFormatsToString()
    {
        // Act
        Result<CardDetails> result = CardDetails.Create(" 4242 ", " Visa ", 5, 2030);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CardLast4.Should().Be("4242");
        result.Value.CardBrand.Should().Be("Visa");
        result.Value.ExpMonth.Should().Be(5);
        result.Value.ExpYear.Should().Be(2030);
        result.Value.ToString().Should().Be("Visa **** 4242 (05/2030)");
    }

    [Fact]
    public void IsExpired_WithPastOrCurrentMonth_ReturnsExpectedResult()
    {
        // Arrange
        CardDetails card = CardDetails.Create("4242", "Visa", 6, 2026).Value;

        // Assert
        card.IsExpired(new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc)).Should().BeTrue();
        card.IsExpired(new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Should().BeTrue();
        card.IsExpired(new DateTime(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc)).Should().BeFalse();
    }

    [Fact]
    public void Equality_WithSameCardFields_ReturnsTrue()
    {
        // Arrange
        CardDetails first = CardDetails.Create("4242", "Visa", 12, 2030).Value;
        CardDetails second = CardDetails.Create("4242", "Visa", 12, 2030).Value;
        CardDetails different = CardDetails.Create("1111", "Visa", 12, 2030).Value;

        // Assert
        first.Should().Be(second);
        first.Should().NotBe(different);
    }
}
