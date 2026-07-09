using AutoMapper;
using Contracts.Events.SensorEvents;
using Contracts.Results;
using Control.Application.Features.Sensors.Commands.SyncSensorUpdated;
using MassTransit;
using MediatR;

namespace Control.Infrastructure.Messaging.Sensor;

internal sealed class SensorUpdatedEventConsumer(ISender sender, IMapper mapper)
    : IConsumer<SensorUpdatedEvent>
{
    public async Task Consume(ConsumeContext<SensorUpdatedEvent> context)
    {
        SyncSensorUpdatedCommand command = mapper.Map<SyncSensorUpdatedCommand>(context.Message);

        Result result = await sender.Send(command, context.CancellationToken);
        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Message);
        }
    }
}
