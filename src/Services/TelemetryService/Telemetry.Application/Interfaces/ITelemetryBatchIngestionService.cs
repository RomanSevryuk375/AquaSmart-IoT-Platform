using BuildingBlocks.Domain.Results;
using BuildingBlocks.IntegrationEvents.Events.Telemetrys;

namespace Telemetry.Application.Interfaces;

public interface ITelemetryBatchIngestionService
{
    Task<Result> IngestAsync(
        Guid controllerId,
        Guid userId,
        IReadOnlyList<TelemetryBatchEventItem> items,
        CancellationToken cancellationToken);
}
