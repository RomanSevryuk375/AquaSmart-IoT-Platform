using Handlers.Callbacks;
using Interfaces;
using Microsoft.Extensions.Logging.Abstractions;

namespace TelegramBot.Application.UnitTests.Application.Handlers.Callbacks;

public class MarkNotificationReadCallbackHandlerTests
{
    private readonly INotificationApiClient _notificationClient = Substitute.For<INotificationApiClient>();
    private readonly ITelegramResponseService _response = Substitute.For<ITelegramResponseService>();
    private readonly MarkNotificationReadCallbackHandler _handler;

    public MarkNotificationReadCallbackHandlerTests()
    {
        _handler = new MarkNotificationReadCallbackHandler(
            _notificationClient,
            _response,
            NullLogger<MarkNotificationReadCallbackHandler>.Instance);
    }

    [Fact]
    public async Task HandleAsync_WithValidNotificationId_MarksAsReadAndRemovesKeyboard()
    {
        // Arrange
        const long chatId = 123L;
        const int messageId = 42;
        const string callbackQueryId = "cq_001";
        var notificationId = Guid.NewGuid();

        // Act
        await _handler.HandleAsync(chatId, messageId, callbackQueryId, notificationId.ToString(), CancellationToken.None);

        // Assert
        await _notificationClient.Received(1).MarkNotificationAsReadAsync(
            chatId, notificationId, Arg.Any<CancellationToken>());

        await _response.Received(1).EditReplyMarkupAsync(
            chatId, messageId, null, Arg.Any<CancellationToken>());

        await _response.Received(1).AnswerCallbackAsync(
            callbackQueryId, Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WithInvalidGuid_AnswersCallbackWithErrorAndSkipsApiCall()
    {
        // Arrange
        const long chatId = 456L;
        const int messageId = 10;
        const string callbackQueryId = "cq_002";

        // Act
        await _handler.HandleAsync(chatId, messageId, callbackQueryId, "not-a-guid", CancellationToken.None);

        // Assert
        await _notificationClient.DidNotReceive()
            .MarkNotificationAsReadAsync(Arg.Any<long>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());

        await _response.Received(1).AnswerCallbackAsync(
            callbackQueryId, Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
