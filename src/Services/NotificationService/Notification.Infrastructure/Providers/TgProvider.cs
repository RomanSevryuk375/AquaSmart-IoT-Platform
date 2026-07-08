// Ignore Spelling: Tg

using Contracts.Options;
using Contracts.Results;
using Microsoft.Extensions.Options;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;

namespace Notification.Infrastructure.Providers;

public sealed class TgProvider(
    HttpClient httpClient,
    IOptions<TelegramOptions> options) : INotificationProvider
{
    private readonly TelegramOptions _settings = options.Value;

    public bool IsEnabled(User user) => user.TgEnable && user.TelegramChatId.HasValue;

    public async Task<Result> SendAsync(
        User user,
        string message,
        CancellationToken cancellationToken = default)
    {
        string token = _settings.BotToken;
        string chatId = user.TelegramChatId!.Value.ToString();

        string url = $"https://api.telegram.org/bot{token}/sendMessage" +
                     $"?chat_id={chatId}&text={Uri.EscapeDataString(message)}";

        try
        {
            using HttpResponseMessage response = await httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return Result.Failure(Error.Failure<TgProvider>(
                    $"Telegram API Error: {response.StatusCode}. Details: {errorContent}"));
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Failure<TgProvider>(
                $"{ex.Message} {ex.StackTrace}"));
        }
    }
}
