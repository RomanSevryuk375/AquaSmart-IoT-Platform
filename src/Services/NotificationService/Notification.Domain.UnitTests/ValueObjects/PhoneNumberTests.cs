using Contracts.Results;
using FluentAssertions;
using Notification.Domain.ValueObjects;

namespace Notification.Domain.UnitTests.ValueObjects;

public class PhoneNumberTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespace_ReturnsFailure(string? invalidPhone)
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
    [InlineData("12345")]
    [InlineData("+37529")]
    [InlineData("+37529123456")]
    [InlineData("+3752912345678")]
    [InlineData("+375171234567")]
    [InlineData("804412345678")]
    [InlineData("+375(29)1234567")]
    [InlineData("80-29-1234567")]
    public void Create_WithInvalidFormat_ReturnsFailure(string invalidPhone)
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
    [InlineData("  80447654321  ", "80447654321")]
    [InlineData("+375331112233", "+375331112233")]
    [InlineData("80259998877", "80259998877")]
    public void Create_WithValidPhone_ReturnsSuccessAndTrimsValue(string input, string expected)
    {
        // Act
        Result<PhoneNumber> result = PhoneNumber.Create(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(expected);
        result.Value.ToString().Should().Be(expected);
    }

    [Fact]
    public void Equals_WithSameValue_ReturnsTrue()
    {
        // Arrange
        PhoneNumber phone1 = PhoneNumber.Create("+375291234567").Value;
        PhoneNumber phone2 = PhoneNumber.Create("  +375291234567  ").Value;

        // Assert
        phone1.Should().Be(phone2);
    }
}
