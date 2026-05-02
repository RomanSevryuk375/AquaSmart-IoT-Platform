using Contracts.Enums;
using Microsoft.EntityFrameworkCore;
using Telemetry.Domain.Entities;
using Telemetry.Domain.Interfaces;

namespace Telemetry.Infrastructure.Repositories;

public sealed class TelemetryAggregateDataRepository(SystemDbContext dbContext) 
    : BaseRepository<TelemetryAggregateEntity>(dbContext), ITelemetryAggregateDataRepository
{
    public async Task<IReadOnlyList<TelemetrySummary>> GetSummaryForPeriodAsync(
        PeriodTypeEnum sourcePeriod, 
        DateTime from, 
        DateTime to, 
        CancellationToken cancellationToken)
    {
        return await Context.TelemetryAggregateData
            .AsNoTracking()
            .Where(x => x.Period == sourcePeriod &&
                        x.PeriodStart >= from &&
                        x.PeriodStart < to)
            .GroupBy(x => x.SensorId)
            .Select(g => new TelemetrySummary
            {
                SensorId = g.Key,
                MinValue = g.Min(x => x.MinValue),
                MaxValue = g.Max(x => x.MaxValue),
                AvgValue = g.Sum(x => x.AvgValue * x.DataPointsCount) / g.Sum(x => x.DataPointsCount),
                Count = g.Sum(x => x.DataPointsCount)
            }).ToListAsync(cancellationToken);
    }

    public async Task DeleteOldRawDataAsync(
        PeriodTypeEnum period,
        DateTime olderThan,
        CancellationToken cancellationToken)
    {
        await Context.TelemetryAggregateData
            .Where(x => x.CreatedAt < olderThan 
                     && x.IsAggregated == false
                     && x.Period == period)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task MarkAsAggregatedAsync(
        List<Guid> sensorIds,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        await Context.TelemetryAggregateData
            .Where(x => sensorIds.Contains(x.SensorId) &&
                        x.CreatedAt >= from &&
                        x.CreatedAt < to)
            .ExecuteUpdateAsync(x => x.SetProperty(p => p.IsAggregated, true), cancellationToken);
    }
}
