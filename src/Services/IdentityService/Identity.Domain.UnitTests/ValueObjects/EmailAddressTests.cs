using Contracts.Results;
using FluentAssertions;
using IdentityService.Domain.ValueObjects;
using Xunit;

namespace Identity.Domain.UnitTests.ValueObjects;

public class EmailAddressTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespace_ReturnsFailure(string? value)
    {
        // Act
        var result = EmailAddress.Create(value!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("EmailAddress.Invalid");
        result.Error.Message.Should().Be("Email address is required.");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Create_WithLengthExceedingLimit_ReturnsFailure()
    {
        // Arrange
        var longEmail = new string('a', 250) + "@test.com"; // 259 chars total

        // Act
        var result = EmailAddress.Create(longEmail);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("EmailAddress.Invalid");
        result.Error.Message.Should().Be("Email address cannot exceed 256 characters.");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData("plainaddress")]
    [InlineData("#@%^%#$@#$@#.com")]
    [InlineData("@example.com")]
    [InlineData("Joe Smith <email@example.com>")]
    [InlineData("email.example.com")]
    [InlineData("email@example@example.com")]
    [InlineData("email@example")]
    public void Create_WithInvalidFormat_ReturnsFailure(string invalidEmail)
    {
        // Act
        var result = EmailAddress.Create(invalidEmail);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("EmailAddress.Invalid");
        result.Error.Message.Should().Be("Invalid email address format.");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData("test@example.com", "test@example.com")]
    [InlineData("  Test@Example.Com  ", "test@example.com")]
    [InlineData("user.name+tag@gmail.com", "user.name+tag@gmail.com")]
    public void Create_WithValidEmail_ReturnsSuccessAndFormatsValue(string input, string expected)
    {
        // Act
        var result = EmailAddress.Create(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(expected);
        result.Value.ToString().Should().Be(expected);
    }
}
