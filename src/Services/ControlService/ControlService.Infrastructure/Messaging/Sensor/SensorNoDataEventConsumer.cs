using Contracts.Events.SensorEvents;
using Contracts.Results;
using Control.Application.Interfaces;
using MassTransit;

namespace Control.Infrastructure.Messaging.Sensor;

internal sealed class SensorNoDataEventConsumer(ISensorService service)
    : IConsumer<SensorNoDataEvent>
{
    public async Task Consume(ConsumeContext<SensorNoDataEvent> context)
    {
        ConsumerResult result = await service.HandleSensorNoDataEventAsync(
            context.Message, context.CancellationToken);

        if (!result.IsSuccess && result.IsRetryable)
        {
            throw new Exception(result.Error);
        }
    }
}
