using Contracts.Results;
using FluentAssertions;
using MassTransit;
using Notification.Application.InternalEvents;
using Notification.Domain.Interfaces;
using Notification.Domain.ValueObjects;
using Notification.Infrastructure.Messaging;
using NSubstitute;

namespace Notification.Infrastructure.IntegrationTests.Messaging;

public class SendEmailCommandConsumerTests
{
    private readonly IEmailProvider _emailProviderMock = Substitute.For<IEmailProvider>();
    private readonly ILogger<SendEmailCommandConsumer> _loggerMock = Substitute.For<ILogger<SendEmailCommandConsumer>>();
    private readonly SendEmailCommandConsumer _consumer;

    public SendEmailCommandConsumerTests()
    {
        _consumer = new SendEmailCommandConsumer(_emailProviderMock, _loggerMock);
    }

    [Fact]
    public async Task Consume_WhenSendAsyncSucceeds_ShouldNotThrow()
    {
        // Arrange
        var command = new SendEmailCommand
        {
            NotificationId = Guid.NewGuid(),
            Email = "user@test.com",
            Message = "Test message"
        };

        ConsumeContext<SendEmailCommand> contextMock = Substitute.For<ConsumeContext<SendEmailCommand>>();
        contextMock.Message.Returns(command);
        contextMock.CancellationToken.Returns(CancellationToken.None);

        _emailProviderMock.SendAsync(
            Arg.Any<NotificationRecipient>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        Func<Task> act = async () => await _consumer.Consume(contextMock);

        // Assert
        await act.Should().NotThrowAsync();

        await _emailProviderMock.Received(1).SendAsync(
            Arg.Is<NotificationRecipient>(r => r.Email == command.Email && r.TgChatId == null),
            command.Message,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_WhenSendAsyncFails_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var command = new SendEmailCommand
        {
            NotificationId = Guid.NewGuid(),
            Email = "user@test.com",
            Message = "Test message"
        };

        ConsumeContext<SendEmailCommand> contextMock = Substitute.For<ConsumeContext<SendEmailCommand>>();
        contextMock.Message.Returns(command);
        contextMock.CancellationToken.Returns(CancellationToken.None);

        var error = Error.Failure("EmailProvider.Error", "SMTP connection failed");
        _emailProviderMock.SendAsync(
            Arg.Any<NotificationRecipient>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(Result.Failure(error));

        // Act
        Func<Task> act = async () => await _consumer.Consume(contextMock);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*SMTP error: {error.Message}*");
    }
}
