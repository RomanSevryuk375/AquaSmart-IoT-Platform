using Contracts.Events.SensorEvents;
using Contracts.Results;

namespace Device.Infrastructure.Messaging;

public sealed class SensorNoDataConsumer(
    ISensorService sensorService) : IConsumer<SensorNoDataEvent>
{
    public async Task Consume(ConsumeContext<SensorNoDataEvent> context)
    {
        ConsumerResult result = await sensorService.SetSensorStateAsync(
            context.Message.SensorId,
            context.Message.State,
            context.CancellationToken);

        if (!result.IsSuccess &&
            result.IsRetryable &&
            result.Error is not null)
        {
            throw new ConsumerException(result.Error);
        }
    }
}
