using BuildingBlocks.Domain.Results;
using MediatR;
using Telemetry.Application.Interfaces;

namespace Telemetry.Application.Features.Telemetry.Commands.IngestDeviceTelemetryBatch;

public sealed class IngestDeviceTelemetryBatchHandler(
    ITelemetryBatchIngestionService ingestionService) : IRequestHandler<IngestDeviceTelemetryBatchCommand, Result>
{
    public Task<Result> Handle(IngestDeviceTelemetryBatchCommand request, CancellationToken cancellationToken) =>
        ingestionService.IngestAsync(
            request.ControllerId,
            request.UserId,
            request.Items,
            cancellationToken);
}
