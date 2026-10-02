using Interfaces;

namespace Handlers.Callbacks;

/// <summary>
/// Handles the <c>cancel_flow</c> and <c>log_skip_notes</c> callback queries.
/// </summary>
public sealed class CancelFlowCallbackHandler(
    IFsmSessionStore sessionStore,
    ITelegramResponseService response)
{
    public async Task HandleCancelAsync(
        long chatId,
        int messageId,
        string callbackQueryId,
        CancellationToken cancellationToken)
    {
        await sessionStore.DeleteAsync(chatId, cancellationToken);
        await response.EditReplyMarkupAsync(chatId, messageId, null, cancellationToken);
        await response.AnswerCallbackAsync(callbackQueryId, "Операция отменена.", cancellationToken);
        await response.SendTextAsync(chatId, "❌ Операция отменена. Возвращаюсь в главное меню.", cancellationToken);
    }

    public async Task HandleSkipNotesAsync(
        long chatId,
        string callbackQueryId,
        CancellationToken cancellationToken) => await response.AnswerCallbackAsync(callbackQueryId, cancellationToken: cancellationToken);// Route empty string through AddLogFsmHandler's notes state — handled externally
}
