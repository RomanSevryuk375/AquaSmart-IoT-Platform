using Contracts.Events.SensorEvents;
using MassTransit;
using Telemetry.Application.Interfaces;

namespace Telemetry.Infrastructure.Messaging.SensorConsumers;

public class SensorStateChangedConsumer(
    ISensorService sensorService) : IConsumer<SensorStateChangedCommand>
{
    public async Task Consume(ConsumeContext<SensorStateChangedCommand> context)
    {
        var result = await sensorService.SetSensorStateAsync(
            context.Message, context.CancellationToken);

        if (!result.IsSuccess && result.IsRetryable)
        {
            throw new Exception(result.Error);
        }
    }
}
