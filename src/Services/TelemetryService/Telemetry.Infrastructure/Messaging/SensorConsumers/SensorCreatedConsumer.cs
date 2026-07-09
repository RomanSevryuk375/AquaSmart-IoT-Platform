using AutoMapper;
using Contracts.Events.SensorEvents;
using Contracts.Results;
using MassTransit;
using MediatR;
using Telemetry.Application.Features.Sensors.Commands.SyncSensorCreated;

namespace Telemetry.Infrastructure.Messaging.SensorConsumers;

public sealed class SensorCreatedConsumer(ISender sender, IMapper mapper) : IConsumer<SensorCreatedEvent>
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
