using BuildingBlocks.Domain.Abstractions;
using Telemetry.Domain.Entities;
using Telemetry.Domain.ValueObjects;

namespace Telemetry.Domain.Interfaces;

public interface ITelemetryRawDataRepository : IRepository<RawTelemetry>
{
    public Task<RawTelemetry?> GetByExternalMessageIdAsync(
        string externalMessageId,
        CancellationToken cancellationToken = default);

    public Task<IReadOnlyDictionary<Guid, TelemetrySummary>> GetSummaryForPeriodAsync(
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<DateTime>> GetUnaggregatedMinuteWindowsAsync(
        DateTime maxCeilingUtc,
        int limit,
        CancellationToken cancellationToken = default);

    public Task MarkAsAggregatedAsync(
        List<Guid> sensorIds,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);
}
