using Contracts.Abstractions;
using Contracts.Enums;
using Telemetry.Domain.Entities;
using Telemetry.Domain.ValueObjects;

namespace Telemetry.Domain.Interfaces;

public interface ITelemetryAggregateDataRepository : IRepository<AggregateTelemetry>
{
    public Task DeleteOldRawDataAsync(
        PeriodType period,
        DateTime olderThan,
        CancellationToken cancellationToken = default);

    public Task<IReadOnlyDictionary<Guid, TelemetrySummary>> GetSummaryForPeriodAsync(
        PeriodType sourcePeriod,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);

    public Task MarkAsAggregatedAsync(
        List<Guid> sensorIds,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);
}
