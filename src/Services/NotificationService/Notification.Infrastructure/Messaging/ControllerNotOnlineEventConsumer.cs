using Contracts.Events.ControllerEvents;
using MassTransit;
using Notification.Application.Interfaces;

namespace Notification.Infrastructure.Messaging;

public sealed class ControllerNotOnlineEventConsumer(
    IControllerAlertSender alertSender) : IConsumer<ControllerNotOnlineEvent>
{
    public async Task Consume(ConsumeContext<ControllerNotOnlineEvent> context)
    {
        var result = await alertSender.SendControllerNotOnlineAlert(
            context.Message, context.CancellationToken);

        if (!result.IsSuccess && result.IsRetryable)
        {
            throw new Exception(result.Error);
        }
    }
}
