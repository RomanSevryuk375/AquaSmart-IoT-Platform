using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Enums;
using Telemetry.Domain.Entities;
using Telemetry.Domain.ValueObjects;

namespace Telemetry.Domain.Interfaces;

public interface ITelemetryAggregateDataRepository : IRepository<AggregateTelemetry>
{
    public Task<IReadOnlyDictionary<Guid, TelemetrySummary>> GetSummaryForPeriodAsync(
        PeriodType sourcePeriod,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);

    public Task<AggregateTelemetry?> GetBySensorAndPeriodAsync(
        Guid sensorId,
        PeriodType period,
        DateTime periodStart,
        CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<DateTime>> GetUnaggregatedWindowsAsync(
        PeriodType sourcePeriod,
        string dateTruncPart,
        DateTime maxCeilingUtc,
        int limit,
        CancellationToken cancellationToken = default);

    public Task MarkAsAggregatedAsync(
        List<Guid> sensorIds,
        PeriodType period,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);
}
