using Contracts.Abstractions;
using Telemetry.Domain.Entities;
using Telemetry.Domain.ValueObjects;

namespace Telemetry.Domain.Interfaces;

public interface ITelemetryRawDataRepository : IRepository<RawTelemetry>
{
    public Task<RawTelemetry?> GetByExternalMessageIdAsync(
        string externalMessageId,
        CancellationToken cancellationToken = default);

    public Task DeleteOldRawDataAsync(
        DateTime olderThan,
        CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<TelemetrySummary>> GetSummaryForPeriodAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);

    public Task MarkAsAggregatedAsync(
        List<Guid> sensorIds,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);
}
