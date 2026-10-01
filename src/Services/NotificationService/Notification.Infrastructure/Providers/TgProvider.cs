using System.Net.Http.Json;
using BuildingBlocks.Domain.Constants;
using BuildingBlocks.Domain.Results;
using Microsoft.Extensions.Options;
using Notification.Domain.Interfaces;
using Notification.Domain.ValueObjects;
using Notification.Infrastructure.Options;

namespace Notification.Infrastructure.Providers;

public sealed class TgProvider(
    HttpClient httpClient,
    IOptions<TelegramBotOptions> options) : ITgProvider
{
    private readonly TelegramBotOptions _settings = options.Value;

    public async Task<Result> SendAsync(
        NotificationRecipient recipient,
        string message,
        object? inlineKeyboard = null,
        CancellationToken cancellationToken = default)
    {
        if (!recipient.TgChatId.HasValue)
        {
            return Result.Failure(Error.Validation(
                ErrorCodes.NotificationProvider.TgChatIdMissing,
                ErrorMessages.NotificationProvider.TgChatIdMissing));
        }

        string token = _settings.BotSecretKey;
        string chatId = recipient.TgChatId.Value.ToString();

        try
        {
            object payload = inlineKeyboard is null
                ? new { chat_id = chatId, text = message, parse_mode = "HTML" }
                : new { chat_id = chatId, text = message, parse_mode = "HTML", reply_markup = inlineKeyboard };

            using HttpResponseMessage response = await httpClient.PostAsJsonAsync(
                $"https://api.telegram.org/bot{token}/sendMessage",
                payload,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return Result.Failure(Error.Failure(
                    ErrorCodes.NotificationProvider.TgProviderError,
                    $"Telegram API Error: {response.StatusCode}. Details: {errorContent}"));
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Failure(
                ErrorCodes.NotificationProvider.TgProviderError,
                ex.Message));
        }
    }

    public async Task<Result> EditMessageReplyMarkupAsync(
        long chatId,
        int messageId,
        object? newKeyboard = null,
        CancellationToken cancellationToken = default)
    {
        string token = _settings.BotSecretKey;

        object payload = new
        {
            chat_id = chatId,
            message_id = messageId,
            reply_markup = newKeyboard
        };

        try
        {
            using HttpResponseMessage response = await httpClient.PostAsJsonAsync(
                $"https://api.telegram.org/bot{token}/editMessageReplyMarkup",
                payload,
                cancellationToken);

            return response.IsSuccessStatusCode
                ? Result.Success()
                : Result.Failure(Error.Failure(
                    ErrorCodes.NotificationProvider.TgProviderError,
                    $"Telegram API Error: {response.StatusCode}"));
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Failure(
                ErrorCodes.NotificationProvider.TgProviderError,
                ex.Message));
        }
    }
}
