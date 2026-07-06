using AutoMapper;
using Contracts.Events.SensorEvents;
using Contracts.Results;
using Control.Application.Features.Sensors.Commands.SyncSensorName;
using MassTransit;
using MediatR;

namespace Control.Infrastructure.Messaging.Sensor;

internal sealed class SensorRenamedEventConsumer(ISender sender, IMapper mapper)
    : IConsumer<SensorRenamedEvent>
{
    public async Task Consume(ConsumeContext<SensorRenamedEvent> context)
    {
        SyncSensorNameCommand command = mapper.Map<SyncSensorNameCommand>(context.Message);

        Result result = await sender.Send(command, context.CancellationToken);
        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Message);
        }
    }
}
