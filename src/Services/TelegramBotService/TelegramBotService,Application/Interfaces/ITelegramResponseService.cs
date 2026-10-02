namespace Interfaces;

/// <summary>
/// Provides a Telegram Bot API client scoped to a specific user (chat).
/// Used by handlers to reply, edit messages, and answer callback queries.
/// </summary>
public interface ITelegramResponseService
{
    /// <summary>Send a plain text message to the chat.</summary>
    public Task SendTextAsync(long chatId, string text, CancellationToken cancellationToken = default);

    /// <summary>Send a message with an inline keyboard.</summary>
    public Task SendWithKeyboardAsync(
        long chatId,
        string text,
        object keyboard,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Answer a callback query (dismisses the "loading" spinner on the button).
    /// Optionally shows a brief pop-up notification.
    /// </summary>
    public Task AnswerCallbackAsync(
        string callbackQueryId,
        string? notificationText = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Edit the reply_markup of an existing message (e.g., remove inline buttons after action).
    /// </summary>
    public Task EditReplyMarkupAsync(
        long chatId,
        int messageId,
        object? newKeyboard = null,
        CancellationToken cancellationToken = default);
}
