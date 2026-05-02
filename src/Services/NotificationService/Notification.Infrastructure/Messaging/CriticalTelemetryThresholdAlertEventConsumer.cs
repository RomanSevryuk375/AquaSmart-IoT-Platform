using Contracts.Events.TelemetryEvents;
using MassTransit;
using Notification.Application.Interfaces;

namespace Notification.Infrastructure.Messaging;

public sealed class CriticalTelemetryThresholdAlertEventConsumer(
    ITelemetryAlertSender alertSender) : IConsumer<CriticalTelemetryThresholdAlertEvent>
{
    public async Task Consume(ConsumeContext<CriticalTelemetryThresholdAlertEvent> context)
    {
        var result = await alertSender.SendTelemetryAlertAsync(
            context.Message, context.CancellationToken);

        if (!result.IsSuccess && result.IsRetryable)
        {
            throw new Exception(result.Error);
        }
    }
}
