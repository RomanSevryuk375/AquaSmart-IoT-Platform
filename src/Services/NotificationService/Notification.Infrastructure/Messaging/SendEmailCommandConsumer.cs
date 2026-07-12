using Contracts.Results;
using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Application.InternalEvents;
using Notification.Domain.Interfaces;
using Notification.Domain.ValueObjects;

namespace Notification.Infrastructure.Messaging;

public sealed class SendEmailCommandConsumer(
    IEmailProvider emailProvider,
    ILogger<SendEmailCommandConsumer> logger) : IConsumer<SendEmailCommand>
{
    public async Task Consume(ConsumeContext<SendEmailCommand> context)
    {
        SendEmailCommand cmd = context.Message;
        var recipient = NotificationRecipient.EmailRecipient(cmd.Email);

        Result result = await emailProvider.SendAsync(recipient, cmd.Message, context.CancellationToken);
        if (result.IsFailure)
        {
            logger.LogWarning("Failed to send Email for Notification {Id}: {Error}",
                cmd.NotificationId, result.Error.Message);
            throw new InvalidOperationException($"SMTP error: {result.Error.Message}");
        }

        logger.LogInformation("Successfully sent Email notification {Id}", cmd.NotificationId);
    }
}
