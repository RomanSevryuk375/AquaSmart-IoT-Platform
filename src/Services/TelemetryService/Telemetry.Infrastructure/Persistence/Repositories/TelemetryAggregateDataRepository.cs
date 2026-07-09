using Contracts.Enums;
using Microsoft.EntityFrameworkCore;
using Telemetry.Domain.Entities;
using Telemetry.Domain.Interfaces;
using Telemetry.Domain.ValueObjects;

namespace Telemetry.Infrastructure.Persistence.Repositories;

public sealed class TelemetryAggregateDataRepository(SystemDbContext dbContext)
    : BaseRepository<AggregateTelemetry>(dbContext), ITelemetryAggregateDataRepository
{
    public async Task<IReadOnlyList<TelemetrySummary>> GetSummaryForPeriodAsync(
        PeriodType sourcePeriod,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        var rawData = await Context.TelemetryAggregateData
            .Where(x => x.Period == sourcePeriod &&
                        x.PeriodStart >= from &&
                        x.PeriodStart < to)
            .GroupBy(x => x.SensorId)
            .Select(g => new
            {
                MinValue = g.Min(x => x.Summary.MinValue),
                AvgValue = g.Sum(x => x.Summary.AvgValue * x.Summary.Count) / g.Sum(x => x.Summary.Count),
                MaxValue = g.Max(x => x.Summary.MaxValue),
                Count = g.Sum(x => x.Summary.Count)
            })
            .ToListAsync(cancellationToken);

        return rawData
            .Select(x => TelemetrySummary.Create(x.MinValue, x.AvgValue, x.MaxValue, x.Count).Value)
            .ToList()
            .AsReadOnly();
    }

    public async Task DeleteOldRawDataAsync(
        PeriodType period,
        DateTime olderThan,
        CancellationToken cancellationToken = default)
    {
        await Context.TelemetryAggregateData
            .Where(x => x.CreatedAt < olderThan
                     && !x.IsAggregated
                     && x.Period == period)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task MarkAsAggregatedAsync(
        List<Guid> sensorIds,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        await Context.TelemetryAggregateData
            .Where(x => sensorIds.Contains(
                        x.SensorId) &&
                        x.CreatedAt >= from &&
                        x.CreatedAt < to)
            .ExecuteUpdateAsync(x => x.SetProperty(p => p.IsAggregated, true), cancellationToken);
    }
}
