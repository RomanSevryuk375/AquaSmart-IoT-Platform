using AutoMapper;
using Contracts.Events.SensorEvents;
using Contracts.Results;
using MassTransit;
using MediatR;
using Telemetry.Application.Features.Sensors.Commands.SyncSensorStateChanged;

namespace Telemetry.Infrastructure.Messaging.SensorConsumers;

public sealed class SensorStateChangedConsumer(ISender sender, IMapper mapper)
    : IConsumer<SensorStateChangedEvent>
{
    public async Task Consume(ConsumeContext<SensorStateChangedEvent> context)
    {
        SyncSensorStateChangedCommand command = mapper.Map<SyncSensorStateChangedCommand>(context.Message);

        Result result = await sender.Send(command, context.CancellationToken);
        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Message);
        }
    }
}
