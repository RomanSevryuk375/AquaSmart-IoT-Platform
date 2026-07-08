using Contracts.Constants;
using Contracts.Results;
using FluentAssertions;
using Notification.Domain.ValueObjects;

namespace Notification.Domain.UnitTests.ValueObjects;

public class MessageTextTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespace_ReturnsFailure(string? invalidMessage)
    {
        // Act
        Result<MessageText> result = MessageText.Create(invalidMessage!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("MessageText.Invalid");
        result.Error.Message.Should().Be("Notification message cannot be empty.");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Create_WithMessageExceedingLimit_ReturnsFailure()
    {
        // Arrange
        string longMessage = new string('A', NotificationConstants.MessageLength + 1);

        // Act
        Result<MessageText> result = MessageText.Create(longMessage);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("MessageText.Invalid");
        result.Error.Message.Should().Be($"Message cannot exceed {NotificationConstants.MessageLength} characters.");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData("Hello World", "Hello World")]
    [InlineData("  Some message with spaces around  ", "Some message with spaces around")]
    public void Create_WithValidMessage_ReturnsSuccessAndTrimsValue(string input, string expected)
    {
        // Act
        Result<MessageText> result = MessageText.Create(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(expected);
        result.Value.ToString().Should().Be(expected);
    }

    [Fact]
    public void Equals_WithSameValue_ReturnsTrue()
    {
        // Arrange
        MessageText message1 = MessageText.Create("Hello World").Value;
        MessageText message2 = MessageText.Create("  Hello World  ").Value;

        // Assert
        message1.Should().Be(message2);
    }
}
