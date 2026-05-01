using Contracts.Events.TelemetryEvents;
using Control.Application.Interfaces;
using MassTransit;

namespace Control.Infrastructure.Messaging.Telemetry;

public class TelemetryReceivedEventConsumer(ITelemetryService service) 
    : IConsumer<TelemetryReceivedEvent>
{
    public async Task Consume(ConsumeContext<TelemetryReceivedEvent> context)
    {
        await service.ProcessTelemetryAsync(
            context.Message, context.CancellationToken);
    }
}
