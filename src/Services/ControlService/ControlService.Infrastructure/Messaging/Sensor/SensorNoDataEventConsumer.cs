using Contracts.Events.SensorEvents;
using Control.Application.Interfaces;
using MassTransit;

namespace Control.Infrastructure.Messaging.Sensor;

public class SensorNoDataEventConsumer(ISensorService service)
    : IConsumer<SensorNoDataEvent>
{
    public async Task Consume(ConsumeContext<SensorNoDataEvent> context)
    {
        await service.HandleSensorNoDataEventAsync(
            context.Message, context.CancellationToken);
    }
}
