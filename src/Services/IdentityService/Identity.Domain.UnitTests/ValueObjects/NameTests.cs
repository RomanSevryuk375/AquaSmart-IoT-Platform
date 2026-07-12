using Contracts.Results;
using FluentAssertions;
using IdentityService.Domain.ValueObjects;

namespace Identity.Domain.UnitTests.ValueObjects;

public class NameTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespaceName_ReturnsFailure(string? invalidName)
    {
        // Act
        Result<Name> result = Name.Create(invalidName!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Name.Invalid");
        result.Error.Message.Should().Be("Name cannot be empty.");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Create_WithNameExceedingLimit_ReturnsFailure()
    {
        // Arrange
        string longName = new string('a', 129);

        // Act
        Result<Name> result = Name.Create(longName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Name.Invalid");
        result.Error.Message.Should().Be("Name cannot exceed 128 characters.");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData("John Doe", "John Doe")]
    [InlineData("   John Doe   ", "John Doe")]
    [InlineData("a", "a")]
    public void Create_WithValidName_ReturnsSuccessAndTrimsValue(string input, string expected)
    {
        // Act
        Result<Name> result = Name.Create(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(expected);
        result.Value.ToString().Should().Be(expected);
    }
}
