using Contracts.Events.SensorEvents;
using Control.Application.Interfaces;
using MassTransit;

namespace Control.Infrastructure.Messaging.Sensor;

public class SensorCreatedEventconsumer(ISensorService service) 
    : IConsumer<SensorCreatedEvent>
{
    public async Task Consume(ConsumeContext<SensorCreatedEvent> context)
    {
        await service.CreateSensorAsync(
            context.Message, context.CancellationToken);
    }
}
