using BuildingBlocks.Domain.Results;
using BuildingBlocks.Domain.ValueObjects;

namespace BuildingBlocks.UnitTests.Domain.ValueObjects;

public class EmailAddressTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespace_ReturnsFailure(string? value)
    {
        // Act
        Result<EmailAddress> result = EmailAddress.Create(value!);

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
        string longEmail = new string('a', 250) + "@test.com"; // 259 chars total

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
    [InlineData("email@example")]
    public void Create_WithInvalidFormat_ReturnsFailure(string invalidEmail)
    {
        // Act
        Result<EmailAddress> result = EmailAddress.Create(invalidEmail);

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
    public void Equals_WithSameNormalizedValue_ReturnsTrue()
    {
        // Arrange
        EmailAddress email1 = EmailAddress.Create("test@example.com").Value;
        EmailAddress email2 = EmailAddress.Create("  TEST@EXAMPLE.COM  ").Value;

        // Assert
        email1.Should().Be(email2);
    }
}
