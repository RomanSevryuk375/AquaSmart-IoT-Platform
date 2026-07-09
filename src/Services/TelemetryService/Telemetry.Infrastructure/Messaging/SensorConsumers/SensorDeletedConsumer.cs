using AutoMapper;
using Contracts.Events.SensorEvents;
using Contracts.Results;
using MassTransit;
using MediatR;
using Telemetry.Application.Features.Sensors.Commands.SyncSensorDeleted;

namespace Telemetry.Infrastructure.Messaging.SensorConsumers;

public sealed class SensorDeletedConsumer(ISender sender, IMapper mapper)
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
