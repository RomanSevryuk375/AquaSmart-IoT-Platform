namespace Billing.Domain.UnitTests.ValueObjects;

public class InvoiceNumberTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespace_ReturnsFailure(string? value)
    {
        // Act
        Result<InvoiceNumber> result = InvoiceNumber.Create(value!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("InvoiceNumber.Invalid");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData("INV-26-00001")]
    [InlineData("INV-2026-123")]
    [InlineData("ABC-2026-00001")]
    [InlineData("INV-2026-ABCDE")]
    public void Create_WithInvalidFormat_ReturnsFailure(string invalidNumber)
    {
        // Act
        Result<InvoiceNumber> result = InvoiceNumber.Create(invalidNumber);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("InvoiceNumber.Invalid");
        result.Error.Message.Should().Be("Invoice number must follow the format INV-YYYY-NNNNN.");
    }

    [Theory]
    [InlineData("inv-2026-00001", "INV-2026-00001")]
    [InlineData("  INV-2026-123456  ", "INV-2026-123456")]
    public void Create_WithValidFormat_ReturnsSuccessAndNormalizesToUpper(string input, string expected)
    {
        // Act
        Result<InvoiceNumber> result = InvoiceNumber.Create(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(expected);
        result.Value.ToString().Should().Be(expected);
    }

    [Theory]
    [InlineData(1999, 1)]
    [InlineData(2101, 1)]
    [InlineData(2026, 0)]
    [InlineData(2026, -5)]
    public void Generate_WithInvalidYearOrSequence_ReturnsFailure(int year, long sequence)
    {
        // Act
        Result<InvoiceNumber> result = InvoiceNumber.Generate(year, sequence);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("InvoiceNumber.Invalid");
    }

    [Fact]
    public void Generate_WithValidData_ReturnsFormattedInvoiceNumber()
    {
        // Act
        Result<InvoiceNumber> result = InvoiceNumber.Generate(2026, 42);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("INV-2026-00042");
    }
}
