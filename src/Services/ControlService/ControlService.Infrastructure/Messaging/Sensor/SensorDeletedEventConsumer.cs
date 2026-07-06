using AutoMapper;
using Contracts.Events.SensorEvents;
using Contracts.Results;
using Control.Application.Features.Sensors.Commands.SyncSensorDeleted;
using MassTransit;
using MediatR;

namespace Control.Infrastructure.Messaging.Sensor;

internal sealed class SensorDeletedEventConsume(ISender sender, IMapper mapper)
    : IConsumer<SensorDeletedEvent>
{
    public async Task Consume(ConsumeContext<SensorDeletedEvent> context)
    {
        SyncSensorDeletedCommand command = mapper.Map<SyncSensorDeletedCommand>(context.Message);

        Result result = await sender.Send(command, context.CancellationToken);
        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Message);
        }
    }
}
