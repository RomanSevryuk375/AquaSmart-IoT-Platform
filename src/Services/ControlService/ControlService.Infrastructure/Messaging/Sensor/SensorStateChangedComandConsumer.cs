using Contracts.Events.SensorEvents;
using Contracts.Results;
using Control.Application.Interfaces;
using MassTransit;

namespace Control.Infrastructure.Messaging.Sensor;

internal sealed class SensorStateChangedComandConsumer(
    ISensorService service) : IConsumer<SensorStateChangedEvent>
{
    public async Task Consume(ConsumeContext<SensorStateChangedEvent> context)
    {
        ConsumerResult result = await service.ChangedStateAsync(
            context.Message, context.CancellationToken);

        if (!result.IsSuccess && result.IsRetryable)
        {
            throw new Exception(result.Error);
        }
    }
}
