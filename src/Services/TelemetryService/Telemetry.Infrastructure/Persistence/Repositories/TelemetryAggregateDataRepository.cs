using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Telemetry.Domain.Entities;
using Telemetry.Domain.Interfaces;
using Telemetry.Domain.ValueObjects;

namespace Telemetry.Infrastructure.Persistence.Repositories;

public sealed class TelemetryAggregateDataRepository(TelemetryDbContext dbContext)
    : BaseRepository<TelemetryDbContext, AggregateTelemetry>(dbContext), ITelemetryAggregateDataRepository
{
    public async Task<IReadOnlyDictionary<Guid, TelemetrySummary>> GetSummaryForPeriodAsync(
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
                SensorId = g.Key,
                MinValue = g.Min(x => x.Summary.MinValue),
                AvgValue = g.Sum(x => x.Summary.AvgValue * x.Summary.Count) / g.Sum(x => x.Summary.Count),
                MaxValue = g.Max(x => x.Summary.MaxValue),
                Count = g.Sum(x => x.Summary.Count)
            })
            .ToListAsync(cancellationToken);

        return rawData.ToDictionary(
            x => x.SensorId,
            x => TelemetrySummary.Create(x.MinValue, x.AvgValue, x.MaxValue, x.Count).Value);
    }

    public async Task<AggregateTelemetry?> GetBySensorAndPeriodAsync(
        Guid sensorId,
        PeriodType period,
        DateTime periodStart,
        CancellationToken cancellationToken = default)
    {
        return await Context.TelemetryAggregateData
            .FirstOrDefaultAsync(
                x => x.SensorId == sensorId &&
                     x.Period == period &&
                     x.PeriodStart == periodStart,
                cancellationToken);
    }

    public async Task<IReadOnlyList<DateTime>> GetUnaggregatedWindowsAsync(
        PeriodType sourcePeriod,
        string dateTruncPart,
        DateTime maxCeilingUtc,
        int limit,
        CancellationToken cancellationToken = default)
    {
        string part = dateTruncPart.ToLowerInvariant() switch
        {
            "hour" => "hour",
            "day" => "day",
            _ => "hour"
        };

        string sql = $$"""
            SELECT DISTINCT date_trunc('{{part}}', period_start) AS "Value"
            FROM telemetry_aggregate_data
            WHERE period = {0}
              AND is_aggregated = false
              AND period_start < {1}
            ORDER BY "Value" ASC
            LIMIT {2}
            """;

        return await Context.Database.SqlQueryRaw<DateTime>(
            sql,
            (int)sourcePeriod,
            maxCeilingUtc,
            limit)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsAggregatedAsync(
        List<Guid> sensorIds,
        PeriodType period,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        await Context.TelemetryAggregateData
            .Where(x => sensorIds.Contains(x.SensorId) &&
                        x.Period == period &&
                        x.PeriodStart >= from &&
                        x.PeriodStart < to &&
                        !x.IsAggregated)
            .ExecuteUpdateAsync(x => x.SetProperty(p => p.IsAggregated, true), cancellationToken);
    }
}
