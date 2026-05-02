using Contracts.Abstractions;
using Telemetry.Domain.Entities;

namespace Telemetry.Domain.Interfaces;

public interface ITelemetryRawDataRepository : IRepository<TelemetryRawEntity>
{
    Task<TelemetryRawEntity?> GetByExternalMessageIdAsync(
        string externalMessageId, 
        CancellationToken cancellationToken);

    Task DeleteOldRawDataAsync(
        DateTime olderThan,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<TelemetrySummary>> GetSummaryForPeriodAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);

    Task MarkAsAggregatedAsync(
        List<Guid> sensorIds,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
