using Contracts.Events.SensorEvents;
using MassTransit;
using Notification.Application.Interfaces;

namespace Notification.Infrastructure.Messaging;

public sealed class SensorNoDataAlertEventConsumer(
    ISensorAlertSender alertSender) : IConsumer<SensorNoDataAlertEvent>
{
    public async Task Consume(ConsumeContext<SensorNoDataAlertEvent> context)
    {
        var result = await alertSender.SendSensorNoDataAlertAsync(
            context.Message, context.CancellationToken);

        if (!result.IsSuccess && result.IsRetryable)
        {
            throw new Exception(result.Error);
        }
    }
}
