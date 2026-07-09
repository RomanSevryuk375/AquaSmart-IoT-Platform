using Contracts.Events.SensorEvents;
using Contracts.Results;
using MassTransit;
using Telemetry.Application.Interfaces;

namespace Telemetry.Infrastructure.Messaging.SensorConsumers;

public class SensorRenamedConsumer(
    ISensorService service) : IConsumer<SensorRenamedEvent>
{
    public async Task Consume(ConsumeContext<SensorRenamedEvent> context)
    {
        ConsumerResult result = await service.SetSensorNameAsync(
            context.Message, context.CancellationToken);

        if (!result.IsSuccess && result.IsRetryable)
        {
            throw new Exception(result.Error);
        }
    }
}
