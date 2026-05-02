using Contracts.Events.SensorEvents;
using MassTransit;
using Telemetry.Application.Interfaces;

namespace Telemetry.Infrastructure.Messaging.SensorConsumers;

public class SensorRenamedConsumer(
    ISensorService service) : IConsumer<SensorRenamedEvent>
{
    public async Task Consume(ConsumeContext<SensorRenamedEvent> context)
    {
        var result = await service.SetSensorNameAsync(
            context.Message, context.CancellationToken);

        if (!result.IsSuccess && result.IsRetryable)
        {
            throw new Exception(result.Error);
        }
    }
}
