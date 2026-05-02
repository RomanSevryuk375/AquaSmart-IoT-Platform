using Contracts.Events.SensorEvents;
using MassTransit;
using Telemetry.Application.Interfaces;

namespace Telemetry.Infrastructure.Messaging.SensorConsumers;

public class SensorCreatedConsumer(
    ISensorService sensorService) : IConsumer<SensorCreatedEvent>
{
    public async Task Consume(ConsumeContext<SensorCreatedEvent> context)
    {
        var result = await sensorService.CreateSensorAsync(
            context.Message, context.CancellationToken);

        if (!result.IsSuccess && result.IsRetryable)
        {
            throw new Exception(result.Error);
        }
    }
}
