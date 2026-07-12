using Contracts.Constants;
using Contracts.Options;
using Contracts.Results;
using Microsoft.Extensions.Options;
using Notification.Domain.Interfaces;
using Notification.Domain.ValueObjects;

namespace Notification.Infrastructure.Providers;

public sealed class TgProvider(
    HttpClient httpClient,
    IOptions<TelegramOptions> options) : ITgProvider
{
    private readonly TelegramOptions _settings = options.Value;

    public async Task<Result> SendAsync(
        NotificationRecipient recipient,
        string message,
        CancellationToken cancellationToken = default)
    {
        if (!recipient.TgChatId.HasValue)
        {
            return Result.Failure(Error.Validation(
                ErrorCodes.NotificationProvider.TgChatIdMissing,
                ErrorMessages.NotificationProvider.TgChatIdMissing));
        }

        string token = _settings.BotToken;
        string chatId = recipient.TgChatId.Value.ToString();

        string url = $"https://api.telegram.org/bot{token}/sendMessage" +
                     $"?chat_id={chatId}&text={Uri.EscapeDataString(message)}";

        try
        {
            using HttpResponseMessage response = await httpClient.GetAsync(url, cancellationToken);

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
}
