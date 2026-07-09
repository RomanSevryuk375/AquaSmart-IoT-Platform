using Microsoft.EntityFrameworkCore;
using Telemetry.Domain.Entities;
using Telemetry.Domain.Interfaces;
using Telemetry.Domain.ValueObjects;

namespace Telemetry.Infrastructure.Persistence.Repositories;

public sealed class TelemetryRawDataRepository(TelemetryDbContext dbContext)
    : BaseRepository<RawTelemetry>(dbContext), ITelemetryRawDataRepository
{
    public async Task<RawTelemetry?> GetByExternalMessageIdAsync(
        string externalMessageId,
        CancellationToken cancellationToken = default)
    {
        return await Context.TelemetryRawData
            .FirstOrDefaultAsync(x =>
                x.ExternalMessageId == externalMessageId, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, TelemetrySummary>> GetSummaryForPeriodAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        var rawData = await Context.TelemetryRawData
            .Where(x => x.RecordedAt >= from && x.RecordedAt < to)
            .GroupBy(x => x.SensorId)
            .Select(g => new
            {
                SensorId = g.Key,
                MinValue = g.Min(x => x.Value),
                AvgValue = g.Average(x => x.Value),
                MaxValue = g.Max(x => x.Value),
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);

        return rawData.ToDictionary(
            x => x.SensorId,
            x => TelemetrySummary.Create(x.MinValue, x.AvgValue, x.MaxValue, x.Count).Value);
    }

    public async Task DeleteOldRawDataAsync(
        DateTime olderThan,
        CancellationToken cancellationToken = default)
    {
        await Context.TelemetryRawData
            .Where(x => x.RecordedAt < olderThan && x.IsAggregated)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task MarkAsAggregatedAsync(
        List<Guid> sensorIds,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        await Context.TelemetryRawData
            .Where(x => sensorIds.Contains(x.SensorId) &&
                        x.RecordedAt >= from &&
                        x.RecordedAt < to)
            .ExecuteUpdateAsync(x => x.SetProperty(p => p.IsAggregated, true), cancellationToken);
    }
}
