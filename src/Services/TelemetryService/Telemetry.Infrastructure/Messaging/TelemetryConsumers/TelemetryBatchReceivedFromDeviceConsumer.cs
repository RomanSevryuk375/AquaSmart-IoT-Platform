using AutoMapper;
using BuildingBlocks.Domain.Results;
using BuildingBlocks.IntegrationEvents.Events.Telemetrys;
using MassTransit;
using MediatR;
using Telemetry.Application.Features.Telemetry.Commands.IngestDeviceTelemetryBatch;

namespace Telemetry.Infrastructure.Messaging.TelemetryConsumers;

public sealed class TelemetryBatchReceivedFromDeviceConsumer(ISender sender, IMapper mapper)
    : IConsumer<TelemetryBatchReceivedFromDeviceEvent>
{
    public async Task Consume(ConsumeContext<TelemetryBatchReceivedFromDeviceEvent> context)
    {
        IngestDeviceTelemetryBatchCommand command = mapper.Map<IngestDeviceTelemetryBatchCommand>(context.Message);

        Result result = await sender.Send(command, context.CancellationToken);
        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Message);
        }
    }
}
