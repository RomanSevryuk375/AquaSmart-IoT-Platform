using Contracts.Events.SensorEvents;
using MassTransit;
using Telemetry.Application.Interfaces;

namespace Telemetry.Infrastructure.Messaging.SensorConsumers;

public class SensorUpdatedConsumer(
    ISensorService sensorService) : IConsumer<SensorUpdatedEvent>
{
    public async Task Consume(ConsumeContext<SensorUpdatedEvent> context)
    {
        var result = await sensorService.UpdateSensorAsync(
            context.Message, context.CancellationToken);

        if (!result.IsSuccess && result.IsRetryable)
        {
            throw new Exception(result.Error);
        }
    }
}
