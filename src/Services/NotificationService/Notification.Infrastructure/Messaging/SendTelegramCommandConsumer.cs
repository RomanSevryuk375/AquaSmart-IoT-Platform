using Contracts.Results;
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

        Result result = await telegramProvider.SendAsync(recipient, cmd.Message, context.CancellationToken);
        if (result.IsFailure)
        {
            logger.LogWarning("Failed to send Telegram message for Notification {Id}: {Error}",
                cmd.NotificationId, result.Error.Message);
            throw new InvalidOperationException($"Telegram API error: {result.Error.Message}");
        }

        logger.LogInformation("Successfully sent Telegram notification {Id}", cmd.NotificationId);
    }
}
