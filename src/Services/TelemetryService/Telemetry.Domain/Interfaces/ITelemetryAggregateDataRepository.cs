using Contracts.Abstractions;
using Contracts.Enums;
using Telemetry.Domain.Entities;

namespace Telemetry.Domain.Interfaces;

public interface ITelemetryAggregateDataRepository : IRepository<TelemetryAggregateEntity>
{
    Task DeleteOldRawDataAsync(
        PeriodType period,
        DateTime olderThan,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<TelemetrySummary>> GetSummaryForPeriodAsync(
        PeriodType sourcePeriod,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);

    Task MarkAsAggregatedAsync(
        List<Guid> sensorIds,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}