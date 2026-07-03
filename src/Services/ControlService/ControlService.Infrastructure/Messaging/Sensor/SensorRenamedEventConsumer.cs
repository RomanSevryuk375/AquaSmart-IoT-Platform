using Contracts.Events.SensorEvents;
using Contracts.Results;
using Control.Application.Interfaces;
using MassTransit;

namespace Control.Infrastructure.Messaging.Sensor;

internal sealed class SensorRenamedEventConsumer(
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
