using Contracts.Events.SensorEvents;
using Control.Application.Interfaces;
using MassTransit;

namespace Control.Infrastructure.Messaging.Sensor;

public class SensorDeletedEventConsume(ISensorService service)
    : IConsumer<SensorDeletedEvent>
{
    public async Task Consume(ConsumeContext<SensorDeletedEvent> context)
    {
        await service.DeletedSensorFromEventAsync(
            context.Message, context.CancellationToken);
    }
}
