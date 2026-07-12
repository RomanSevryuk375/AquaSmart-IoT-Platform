using Contracts.Results;
using FluentAssertions;
using MassTransit;
using Notification.Application.InternalEvents;
using Notification.Domain.Interfaces;
using Notification.Domain.ValueObjects;
using Notification.Infrastructure.Messaging;
using NSubstitute;

namespace Notification.Infrastructure.IntegrationTests.Messaging;

public class SendTelegramCommandConsumerTests
{
    private readonly ITgProvider _tgProviderMock = Substitute.For<ITgProvider>();
    private readonly ILogger<SendTelegramCommandConsumer> _loggerMock = Substitute.For<ILogger<SendTelegramCommandConsumer>>();
    private readonly SendTelegramCommandConsumer _consumer;

    public SendTelegramCommandConsumerTests()
    {
        _consumer = new SendTelegramCommandConsumer(_tgProviderMock, _loggerMock);
    }

    [Fact]
    public async Task Consume_WhenSendAsyncSucceeds_ShouldNotThrow()
    {
        // Arrange
        var command = new SendTelegramCommand
        {
            NotificationId = Guid.NewGuid(),
            ChatId = 123456789,
            Message = "Test message"
        };

        ConsumeContext<SendTelegramCommand> contextMock = Substitute.For<ConsumeContext<SendTelegramCommand>>();
        contextMock.Message.Returns(command);
        contextMock.CancellationToken.Returns(CancellationToken.None);

        _tgProviderMock.SendAsync(
            Arg.Any<NotificationRecipient>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        Func<Task> act = async () => await _consumer.Consume(contextMock);

        // Assert
        await act.Should().NotThrowAsync();

        await _tgProviderMock.Received(1).SendAsync(
            Arg.Is<NotificationRecipient>(r => r.TgChatId == command.ChatId && r.Email == null),
            command.Message,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_WhenSendAsyncFails_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var command = new SendTelegramCommand
        {
            NotificationId = Guid.NewGuid(),
            ChatId = 123456789,
            Message = "Test message"
        };

        ConsumeContext<SendTelegramCommand> contextMock = Substitute.For<ConsumeContext<SendTelegramCommand>>();
        contextMock.Message.Returns(command);
        contextMock.CancellationToken.Returns(CancellationToken.None);

        var error = Error.Failure("TgProvider.Error", "API request timed out");
        _tgProviderMock.SendAsync(
            Arg.Any<NotificationRecipient>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(Result.Failure(error));

        // Act
        Func<Task> act = async () => await _consumer.Consume(contextMock);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*Telegram API error: {error.Message}*");
    }
}
