using Contracts.Events.SensorEvents;
using Control.Application.Interfaces;
using MassTransit;

namespace Control.Infrastructure.Messaging.Sensor;

public class SensorStateChangedComandConsumer(
    ISensorService service) : IConsumer<SensorStateChangedEvent>
{
    public async Task Consume(ConsumeContext<SensorStateChangedEvent> context)
    {
        await service.ChangedStateAsync(
            context.Message, context.CancellationToken);
    }
}
