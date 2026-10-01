using BuildingBlocks.Domain.Results;
using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Application.InternalEvents;
using Notification.Domain.Interfaces;
using Notification.Domain.ValueObjects;

namespace Notification.Infrastructure.Messaging;

public sealed class SendTelegramCommandConsumer(
    ITgProvider telegramProvider,
    ILogger<SendTelegramCommandConsumer> logger) : IConsumer<SendTelegramCommand>
{
    public async Task Consume(ConsumeContext<SendTelegramCommand> context)
    {
        SendTelegramCommand cmd = context.Message;
        var recipient = NotificationRecipient.TelegramRecipient(cmd.ChatId);

        object? keyboard = BuildInlineKeyboard(cmd);

        Result result = await telegramProvider.SendAsync(recipient, cmd.Message, keyboard, context.CancellationToken);
        if (result.IsFailure)
        {
            logger.LogWarning("Failed to send Telegram message for Notification {Id}: {Error}",
                cmd.NotificationId, result.Error.Message);
            throw new InvalidOperationException($"Telegram API error: {result.Error.Message}");
        }

        logger.LogInformation("Successfully sent Telegram notification {Id}", cmd.NotificationId);
    }

    private static object? BuildInlineKeyboard(SendTelegramCommand cmd)
    {
        if (cmd.ReminderId.HasValue)
        {
            return new
            {
                inline_keyboard = new[]
                {
                    new[]
                    {
                        new
                        {
                            text = "✅ Выполнено",
                            callback_data = $"done_reminder:{cmd.ReminderId.Value}"
                        }
                    }
                }
            };
        }

        return new
        {
            inline_keyboard = new[]
            {
                new[]
                {
                    new
                    {
                        text = "👁 Отметить прочитанным",
                        callback_data = $"read_notice:{cmd.NotificationId}"
                    }
                }
            }
        };
    }
}
