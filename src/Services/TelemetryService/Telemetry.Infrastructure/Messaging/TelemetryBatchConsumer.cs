using AutoMapper;
using Contracts.Events.TelemetryEvents;
using Contracts.Results;
using MassTransit;
using MediatR;
using Telemetry.Application.Features.Telemetry.Commands.AddTelemetryBatch;

namespace Telemetry.Infrastructure.Messaging;

public sealed class TelemetryBatchConsumer(ISender sender, IMapper mapper)
    : IConsumer<TelemetryBatchEvent>
{
    public async Task Consume(ConsumeContext<TelemetryBatchEvent> context)
    {
        AddTelemetryBatchCommand command = mapper.Map<AddTelemetryBatchCommand>(context.Message);

        Result result = await sender.Send(command, context.CancellationToken);
        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Message);
        }
    }
}
