using System.Net.Http.Json;
using Interfaces;
using Microsoft.Extensions.Options;
using Options;

namespace TelegramBotService.Infrastructure.Telegram;

/// <summary>
/// Thin wrapper over the raw Telegram Bot API HTTP calls.
/// Implements <see cref="ITelegramResponseService"/> so handlers stay testable
/// without the Telegram.Bot SDK types.
/// </summary>
internal sealed class TelegramResponseService(
    HttpClient httpClient,
    IOptions<TelegramBotOptions> options) : ITelegramResponseService
{
    private readonly string _token = options.Value.BotToken;

    public async Task SendTextAsync(long chatId, string text, CancellationToken cancellationToken = default)
    {
        await httpClient.PostAsJsonAsync(
            $"https://api.telegram.org/bot{_token}/sendMessage",
            new { chat_id = chatId, text, parse_mode = "HTML" },
            cancellationToken);
    }

    public async Task SendWithKeyboardAsync(
        long chatId,
        string text,
        object keyboard,
        CancellationToken cancellationToken = default)
    {
        await httpClient.PostAsJsonAsync(
            $"https://api.telegram.org/bot{_token}/sendMessage",
            new { chat_id = chatId, text, parse_mode = "HTML", reply_markup = keyboard },
            cancellationToken);
    }

    public async Task AnswerCallbackAsync(
        string callbackQueryId,
        string? notificationText = null,
        CancellationToken cancellationToken = default)
    {
        await httpClient.PostAsJsonAsync(
            $"https://api.telegram.org/bot{_token}/answerCallbackQuery",
            new { callback_query_id = callbackQueryId, text = notificationText, show_alert = false },
            cancellationToken);
    }

    public async Task EditReplyMarkupAsync(
        long chatId,
        int messageId,
        object? newKeyboard = null,
        CancellationToken cancellationToken = default)
    {
        await httpClient.PostAsJsonAsync(
            $"https://api.telegram.org/bot{_token}/editMessageReplyMarkup",
            new { chat_id = chatId, message_id = messageId, reply_markup = newKeyboard },
            cancellationToken);
    }
}
