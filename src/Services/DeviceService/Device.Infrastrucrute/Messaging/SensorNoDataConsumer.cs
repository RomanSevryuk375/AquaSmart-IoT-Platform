using Contracts.Events.SensorEvents;
using Contracts.Results;
using Device.Application.Features.Sensors.Command.SetSensorState;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Device.Infrastructure.Messaging;

public sealed class SensorNoDataConsumer(
    ISender sender,
    ILogger<SensorNoDataConsumer> logger)
    : IConsumer<SensorNoDataEvent>
{
    public async Task Consume(ConsumeContext<SensorNoDataEvent> context)
    {
        logger.LogInformation("Received {EventName} for Sensor {SensorId}", nameof(SensorNoDataEvent), context.Message.SensorId);

        var command = new SetSensorStateCommand
        {
            SensorId = context.Message.SensorId,
            SensorState = context.Message.State,
        };

        Result result = await sender.Send(command, context.CancellationToken);
        if (!result.IsSuccess && result.Error is not null)
        {
            var exception = new ConsumerException(result.Error.Message);
            logger.LogError(exception, "Error occurred processing {EventName} for Sensor {SensorId}: {ErrorMessage}", nameof(SensorNoDataEvent), context.Message.SensorId, result.Error.Message);
            throw exception;
        }
    }
}
