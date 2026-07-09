using AutoMapper;
using Contracts.Events.SensorEvents;
using Contracts.Results;
using MassTransit;
using MediatR;
using Telemetry.Application.Features.Sensors.Commands.SyncSensorNameChanged;

namespace Telemetry.Infrastructure.Messaging.SensorConsumers;

public sealed class SensorRenamedConsumer(ISender sender, IMapper mapper)
    : IConsumer<SensorRenamedEvent>
{
    public async Task Consume(ConsumeContext<SensorRenamedEvent> context)
    {
        SyncSensorNameChangedCommand command = mapper.Map<SyncSensorNameChangedCommand>(context.Message);

        Result result = await sender.Send(command, context.CancellationToken);
        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Message);
        }
    }
}
