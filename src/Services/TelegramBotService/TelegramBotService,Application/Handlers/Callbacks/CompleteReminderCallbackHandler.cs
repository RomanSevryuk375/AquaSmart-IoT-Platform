using Interfaces;
using Microsoft.Extensions.Logging;

namespace Handlers.Callbacks;

/// <summary>
/// Handles the <c>done_reminder:{reminderId}</c> callback query.
/// Marks the reminder as complete and updates the message to show the next due date.
/// </summary>
public sealed class CompleteReminderCallbackHandler(
    INotificationApiClient notificationClient,
    ITelegramResponseService response,
    ILogger<CompleteReminderCallbackHandler> logger)
{
    public async Task HandleAsync(
        long chatId,
        int messageId,
        string callbackQueryId,
        string reminderIdRaw,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(reminderIdRaw, out Guid reminderId))
        {
            await response.AnswerCallbackAsync(callbackQueryId, "❌ Некорректный ID напоминания.", cancellationToken);
            return;
        }

        try
        {
            DateTime nextDue = await notificationClient.CompleteReminderAsync(chatId, reminderId, cancellationToken);

            await response.EditReplyMarkupAsync(chatId, messageId, newKeyboard: null, cancellationToken);

            await response.AnswerCallbackAsync(
                callbackQueryId,
                $"✅ Выполнено! Следующий срок: {nextDue:dd.MM.yyyy}",
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to complete reminder {ReminderId} for chatId={ChatId}",
                reminderId, chatId);
            await response.AnswerCallbackAsync(callbackQueryId, "⚠️ Не удалось отметить выполненным. Попробуй позже.", cancellationToken);
        }
    }
}
