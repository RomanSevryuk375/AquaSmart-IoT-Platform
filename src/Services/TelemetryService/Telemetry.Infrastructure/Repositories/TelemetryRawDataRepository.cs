using Microsoft.EntityFrameworkCore;
using Telemetry.Domain.Entities;
using Telemetry.Domain.Interfaces;

namespace Telemetry.Infrastructure.Repositories;

public sealed class TelemetryRawDataRepository(SystemDbContext dbContext)
    : BaseRepository<TelemetryRawEntity>(dbContext), ITelemetryRawDataRepository
{
    public async Task<TelemetryRawEntity?> GetByExternalMessageIdAsync(
        string externalMessageId, 
        CancellationToken cancellationToken = default)
    {
        return await Context.TelemetryRawData
            .AsNoTracking()
            .FirstOrDefaultAsync(x => 
                x.ExternalMessageId == externalMessageId, cancellationToken);
    }

    public async Task<IReadOnlyList<TelemetrySummary>> GetSummaryForPeriodAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        return await Context.TelemetryRawData
            .AsNoTracking()
            .Where(x => x.RecordedAt >= from && x.RecordedAt < to)
            .GroupBy(x => x.SensorId)
            .Select(g => new TelemetrySummary {
                SensorId = g.Key,
                MinValue = g.Min(x => x.Value),
                MaxValue = g.Max(x => x.Value),
                AvgValue = g.Average(x => x.Value),
                Count = g.Count()
            }).ToListAsync(cancellationToken);
    }

    public async Task DeleteOldRawDataAsync(
        DateTime olderThan, 
        CancellationToken cancellationToken)
    {
        await Context.TelemetryRawData
            .Where(x => x.RecordedAt < olderThan && x.IsAggregated == true)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task MarkAsAggregatedAsync(
        List<Guid> sensorIds,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        await Context.TelemetryRawData
            .Where(x => sensorIds.Contains(x.SensorId) &&
                        x.RecordedAt >= from &&
                        x.RecordedAt < to)
            .ExecuteUpdateAsync(x => x.SetProperty(p => p.IsAggregated, true), cancellationToken);
    }
}
