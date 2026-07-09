using Contracts.Results;
using FluentAssertions;
using Notification.Domain.ValueObjects;

namespace Notification.Domain.UnitTests.ValueObjects;

public class EmailAddressTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespaceEmail_ReturnsFailure(string? invalidEmail)
    {
        // Act
        Result<EmailAddress> result = EmailAddress.Create(invalidEmail!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("EmailAddress.Invalid");
        result.Error.Message.Should().Be("Email address is required.");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Create_WithEmailExceedingLengthLimit_ReturnsFailure()
    {
        // Arrange
        string longEmail = new string('a', 257) + "@test.com";

        // Act
        Result<EmailAddress> result = EmailAddress.Create(longEmail);

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
    public void Create_WithInvalidFormat_ReturnsFailure(string invalidFormat)
    {
        // Act
        Result<EmailAddress> result = EmailAddress.Create(invalidFormat);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("EmailAddress.Invalid");
        result.Error.Message.Should().Be("Invalid email address format.");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData("test@example.com", "test@example.com")]
    [InlineData("  TEST@example.com  ", "test@example.com")]
    [InlineData("user.name+tag@domain.co.uk", "user.name+tag@domain.co.uk")]
    public void Create_WithValidEmail_ReturnsSuccessAndFormatsValue(string input, string expected)
    {
        // Act
        Result<EmailAddress> result = EmailAddress.Create(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(expected);
        result.Value.ToString().Should().Be(expected);
    }

    [Fact]
    public void Equals_WithSameValue_ReturnsTrue()
    {
        // Arrange
        EmailAddress email1 = EmailAddress.Create("test@example.com").Value;
        EmailAddress email2 = EmailAddress.Create("  TEST@EXAMPLE.COM  ").Value;

        // Assert
        email1.Should().Be(email2);
    }
}
