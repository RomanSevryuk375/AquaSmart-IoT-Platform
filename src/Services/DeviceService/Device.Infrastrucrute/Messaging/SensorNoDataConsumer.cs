using Contracts.Events.SensorEvents;
using Device.Application.Interfaces;

namespace Device.Infrastructure.Messaging;

public sealed class SensorNoDataConsumer(
    ISensorService sensorService) : IConsumer<SensorNoDataEvent>
{
    public async Task Consume(ConsumeContext<SensorNoDataEvent> context)
    {
        await sensorService.SetSensorStateAsync(
            context.Message.SensorId,
            context.Message.State,
            context.CancellationToken);
    }
}
