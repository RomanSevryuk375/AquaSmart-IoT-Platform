using AutoMapper;
using Contracts.Events.TelemetryEvents;
using Contracts.Results;
using Control.Application.Features.Telemetry.Commands.ProcessTelemetry;
using MassTransit;
using MediatR;

namespace Control.Infrastructure.Messaging.Telemetry;

internal sealed class TelemetryReceivedEventConsumer(ISender sender, IMapper mapper)
    : IConsumer<TelemetryReceivedEvent>
{
    public async Task Consume(ConsumeContext<TelemetryReceivedEvent> context)
    {
        ProcessTelemetryCommand command = mapper.Map<ProcessTelemetryCommand>(context.Message);

        Result result = await sender.Send(command, context.CancellationToken);
        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Message);
        }
    }
}
