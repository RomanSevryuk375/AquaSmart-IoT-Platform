using Contracts.Events.SensorEvents;
using Control.Application.Interfaces;
using MassTransit;

namespace Control.Infrastructure.Messaging.Sensor;

public class SensorUpdatedEventConsumer(ISensorService service) 
    : IConsumer<SensorUpdatedEvent>
{
    public async Task Consume(ConsumeContext<SensorUpdatedEvent> context)
    {
        await service.UpdatedSensorAsync(
            context.Message, context.CancellationToken);
    }
}
