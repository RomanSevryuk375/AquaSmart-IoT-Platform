using AutoMapper;
using Contracts.Events.SensorEvents;
using Contracts.Results;
using Control.Application.Features.Sensors.Commands.SyncSensorCreated;
using MassTransit;
using MediatR;

namespace Control.Infrastructure.Messaging.Sensor;

internal sealed class SensorCreatedEventconsumer(ISender sender, IMapper mapper)
    : IConsumer<SensorCreatedEvent>
{
    public async Task Consume(ConsumeContext<SensorCreatedEvent> context)
    {
        SyncSensorCreatedCommand command = mapper.Map<SyncSensorCreatedCommand>(context.Message);

        Result result = await sender.Send(command, context.CancellationToken);
        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Message);
        }
    }
}
