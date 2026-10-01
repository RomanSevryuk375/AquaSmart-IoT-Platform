using Handlers.Callbacks;
using Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute.ExceptionExtensions;

namespace TelegramBot.Application.UnitTests.Application.Handlers.Callbacks;

public class CompleteReminderCallbackHandlerTests
{
    private readonly INotificationApiClient _notificationClient = Substitute.For<INotificationApiClient>();
    private readonly ITelegramResponseService _response = Substitute.For<ITelegramResponseService>();
    private readonly CompleteReminderCallbackHandler _handler;

    public CompleteReminderCallbackHandlerTests()
    {
        _handler = new CompleteReminderCallbackHandler(
            _notificationClient,
            _response,
            NullLogger<CompleteReminderCallbackHandler>.Instance);
    }

    [Fact]
    public async Task HandleAsync_WithValidReminderId_CompletesReminderAndRemovesKeyboard()
    {
        // Arrange
        const long chatId = 200L;
        const int messageId = 55;
        const string callbackQueryId = "cq_100";
        var reminderId = Guid.NewGuid();
        DateTime nextDue = DateTime.UtcNow.AddDays(7);

        _notificationClient.CompleteReminderAsync(chatId, reminderId, Arg.Any<CancellationToken>())
            .Returns(nextDue);

        // Act
        await _handler.HandleAsync(chatId, messageId, callbackQueryId, reminderId.ToString(), CancellationToken.None);

        // Assert
        await _notificationClient.Received(1).CompleteReminderAsync(
            chatId, reminderId, Arg.Any<CancellationToken>());

        await _response.Received(1).EditReplyMarkupAsync(
            chatId, messageId, null, Arg.Any<CancellationToken>());

        await _response.Received(1).AnswerCallbackAsync(
            callbackQueryId,
            Arg.Is<string>(s => s.Contains(nextDue.ToString("dd.MM.yyyy"))),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WithInvalidReminderId_AnswersCallbackWithErrorAndSkipsApiCall()
    {
        // Arrange
        const long chatId = 201L;
        const int messageId = 12;
        const string callbackQueryId = "cq_101";

        // Act
        await _handler.HandleAsync(chatId, messageId, callbackQueryId, "bad-guid", CancellationToken.None);

        // Assert
        await _notificationClient.DidNotReceive()
            .CompleteReminderAsync(Arg.Any<long>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());

        await _response.Received(1).AnswerCallbackAsync(
            callbackQueryId, Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenApiThrows_AnswersCallbackWithWarning()
    {
        // Arrange
        const long chatId = 202L;
        const int messageId = 33;
        const string callbackQueryId = "cq_102";
        var reminderId = Guid.NewGuid();

        _notificationClient.CompleteReminderAsync(chatId, reminderId, Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        // Act
        await _handler.HandleAsync(chatId, messageId, callbackQueryId, reminderId.ToString(), CancellationToken.None);

        // Assert — should not propagate, should answer with error notification
        await _response.Received(1).AnswerCallbackAsync(
            callbackQueryId, Arg.Any<string>(), Arg.Any<CancellationToken>());

        // Should NOT have removed the keyboard (request failed)
        await _response.DidNotReceive().EditReplyMarkupAsync(
            chatId, messageId, null, Arg.Any<CancellationToken>());
    }
}
