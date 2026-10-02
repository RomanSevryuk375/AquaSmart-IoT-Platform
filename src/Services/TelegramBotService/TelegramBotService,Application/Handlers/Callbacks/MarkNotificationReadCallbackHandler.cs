using Interfaces;
using Microsoft.Extensions.Logging;

namespace Handlers.Callbacks;

/// <summary>
/// Handles the <c>read_notice:{notificationId}</c> callback query.
/// Marks the notification as read and removes the inline button from the message.
/// </summary>
public sealed class MarkNotificationReadCallbackHandler(
    INotificationApiClient notificationClient,
    ITelegramResponseService response,
    ILogger<MarkNotificationReadCallbackHandler> logger)
{
    public async Task HandleAsync(
        long chatId,
        int messageId,
        string callbackQueryId,
        string notificationIdRaw,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(notificationIdRaw, out Guid notificationId))
        {
            await response.AnswerCallbackAsync(callbackQueryId, "❌ Некорректный ID уведомления.", cancellationToken);
            return;
        }

        try
        {
            await notificationClient.MarkNotificationAsReadAsync(chatId, notificationId, cancellationToken);

            await response.EditReplyMarkupAsync(chatId, messageId, newKeyboard: null, cancellationToken);

            await response.AnswerCallbackAsync(callbackQueryId, "✅ Отмечено как прочитанное", cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to mark notification {NotificationId} as read for chatId={ChatId}",
                notificationId, chatId);
            await response.AnswerCallbackAsync(callbackQueryId, "⚠️ Не удалось отметить. Попробуй позже.", cancellationToken);
        }
    }
}
