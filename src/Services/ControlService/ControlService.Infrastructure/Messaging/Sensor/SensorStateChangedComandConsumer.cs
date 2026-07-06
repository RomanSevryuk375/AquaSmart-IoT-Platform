using AutoMapper;
using Contracts.Events.SensorEvents;
using Contracts.Results;
using Control.Application.Features.Sensors.Commands.SyncSensorState;
using MassTransit;
using MediatR;

namespace Control.Infrastructure.Messaging.Sensor;

internal sealed class SensorStateChangedComandConsumer(ISender sender, IMapper mapper)
    : IConsumer<SensorStateChangedEvent>
{
    public async Task Consume(ConsumeContext<SensorStateChangedEvent> context)
    {
        SyncSensorStateCommand command = mapper.Map<SyncSensorStateCommand>(context.Message);

        Result result = await sender.Send(command, context.CancellationToken);
        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Message);
        }
    }
}
