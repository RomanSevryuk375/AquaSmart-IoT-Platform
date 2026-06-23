using Contracts.Events.SensorEvents;
using Contracts.Results;
using Device.Application.Features.Sensors.Command.SetSensorState;
using MediatR;

namespace Device.Infrastructure.Messaging;

public sealed class SensorNoDataConsumer(ISender sender)
    : IConsumer<SensorNoDataEvent>
{
    public async Task Consume(ConsumeContext<SensorNoDataEvent> context)
    {
        var command = new SetSensorStateCommand
        {
            SensorId = context.Message.SensorId,
            SensorState = context.Message.State,
        };

        Result result = await sender.Send(command, context.CancellationToken);
        if (!result.IsSuccess && result.Error is not null)
        {
            throw new ConsumerException(result.Error.Message);
        }
    }
}
