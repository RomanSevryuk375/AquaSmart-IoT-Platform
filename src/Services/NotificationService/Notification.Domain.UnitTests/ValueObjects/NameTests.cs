using Contracts.Constants;
using Contracts.Results;
using FluentAssertions;
using Notification.Domain.ValueObjects;

namespace Notification.Domain.UnitTests.ValueObjects;

public class NameTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespace_ReturnsFailure(string? invalidName)
    {
        // Act
        Result<Name> result = Name.Create(invalidName!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Name.Invalid");
        result.Error.Message.Should().Be(ControlValidationMessages.NameCannotBeEmpty);
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Create_WithNameExceedingLimit_ReturnsFailure()
    {
        // Arrange
        string longName = new string('A', CommonConstants.NameLength + 1);

        // Act
        Result<Name> result = Name.Create(longName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Name.Invalid");
        result.Error.Message.Should().Be(string.Format(
            ControlValidationMessages.NameCannotExceedLimitFormat, CommonConstants.NameLength));
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData("Water Level Reminder", "Water Level Reminder")]
    [InlineData("  Task Name  ", "Task Name")]
    public void Create_WithValidName_ReturnsSuccessAndTrimsValue(string input, string expected)
    {
        // Act
        Result<Name> result = Name.Create(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(expected);
        result.Value.ToString().Should().Be(expected);
    }

    [Fact]
    public void Equals_WithSameValue_ReturnsTrue()
    {
        // Arrange
        Name name1 = Name.Create("Water Level").Value;
        Name name2 = Name.Create("  Water Level  ").Value;

        // Assert
        name1.Should().Be(name2);
    }
}
