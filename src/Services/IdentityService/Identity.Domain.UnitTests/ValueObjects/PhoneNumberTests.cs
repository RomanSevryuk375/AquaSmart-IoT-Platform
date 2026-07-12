using Contracts.Results;
using FluentAssertions;
using IdentityService.Domain.ValueObjects;

namespace Identity.Domain.UnitTests.ValueObjects;

public class PhoneNumberTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespacePhoneNumber_ReturnsFailure(string? invalidPhone)
    {
        // Act
        Result<PhoneNumber> result = PhoneNumber.Create(invalidPhone!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PhoneNumber.Invalid");
        result.Error.Message.Should().Be("Phone number is required.");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData("+37529123456")] // Too short
    [InlineData("+3752912345678")] // Too long
    [InlineData("80171234567")] // Invalid operator code (17 is not supported)
    [InlineData("+37544123a567")] // Contains letters
    [InlineData("+12345678900")] // Invalid country prefix
    [InlineData("375291234567")] // Missing leading + or 80
    public void Create_WithInvalidPhoneNumberFormat_ReturnsFailure(string invalidPhone)
    {
        // Act
        Result<PhoneNumber> result = PhoneNumber.Create(invalidPhone);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PhoneNumber.Invalid");
        result.Error.Message.Should().Be("Phone number should be in format +375XXXXXXXXX or 80XXXXXXXXX.");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData("+375291234567", "+375291234567")]
    [InlineData("  +375447654321  ", "+375447654321")] // Trimmed
    [InlineData("80339998877", "80339998877")]
    [InlineData("80251112233", "80251112233")]
    public void Create_WithValidPhoneNumberFormat_ReturnsSuccessAndTrimsValue(string input, string expected)
    {
        // Act
        Result<PhoneNumber> result = PhoneNumber.Create(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(expected);
        result.Value.ToString().Should().Be(expected);
    }
}
