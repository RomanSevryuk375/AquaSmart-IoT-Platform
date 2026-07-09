using AutoMapper;
using Contracts.Events.SensorEvents;
using Contracts.Results;
using MassTransit;
using MediatR;
using Telemetry.Application.Features.Sensors.Commands.SyncSensorUpdated;

namespace Telemetry.Infrastructure.Messaging.SensorConsumers;

public sealed class SensorUpdatedConsumer(ISender sender, IMapper mapper)
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
