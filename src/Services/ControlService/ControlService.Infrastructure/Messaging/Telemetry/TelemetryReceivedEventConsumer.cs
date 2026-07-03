using Contracts.Events.TelemetryEvents;
using Contracts.Results;
using Control.Application.Interfaces;
using MassTransit;

namespace Control.Infrastructure.Messaging.Telemetry;

internal sealed class TelemetryReceivedEventConsumer(ITelemetryService service)
    : IConsumer<TelemetryReceivedEvent>
{
    public async Task Consume(ConsumeContext<TelemetryReceivedEvent> context)
    {
        ConsumerResult result = await service.ProcessTelemetryAsync(
            context.Message, context.CancellationToken);

        if (!result.IsSuccess && result.IsRetryable)
        {
            throw new Exception(result.Error);
        }
    }
}
