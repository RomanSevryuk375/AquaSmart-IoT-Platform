using BuildingBlocks.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Telemetry.Domain.Entities;
using Telemetry.Domain.Interfaces;
using Telemetry.Domain.ValueObjects;

namespace Telemetry.Infrastructure.Persistence.Repositories;

public sealed class TelemetryRawDataRepository(TelemetryDbContext dbContext)
    : BaseRepository<TelemetryDbContext, RawTelemetry>(dbContext), ITelemetryRawDataRepository
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

    public async Task<IReadOnlyList<DateTime>> GetUnaggregatedMinuteWindowsAsync(
        DateTime maxCeilingUtc,
        int limit,
        CancellationToken cancellationToken = default)
    {
        return await Context.Database.SqlQueryRaw<DateTime>(
            """
            SELECT DISTINCT date_trunc('minute', recorded_at) AS "Value"
            FROM telemetry_raw_data
            WHERE is_aggregated = false
              AND recorded_at < {0}
            ORDER BY "Value" ASC
            LIMIT {1}
            """,
            maxCeilingUtc,
            limit)
            .ToListAsync(cancellationToken);
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
                        x.RecordedAt < to &&
                        !x.IsAggregated)
            .ExecuteUpdateAsync(x => x.SetProperty(p => p.IsAggregated, true), cancellationToken);
    }

    public async Task SaveBatchWithDuplicateHandlingAsync(
        CancellationToken cancellationToken = default)
    {
        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? tx = Context.Database.CurrentTransaction;

        if (tx is not null)
        {
            const string BatchSavepoint = "telemetry_batch";
            await tx.CreateSavepointAsync(BatchSavepoint, cancellationToken);

            try
            {
                await Context.SaveChangesAsync(cancellationToken);
                return;
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                await tx.RollbackToSavepointAsync(BatchSavepoint, cancellationToken);
            }
        }
        else
        {
            try
            {
                await Context.SaveChangesAsync(cancellationToken);
                return;
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                // Fall back to per-item save with duplicate detection below.
            }
        }

        await RetryPendingEntriesIndividuallyAsync(tx, cancellationToken);
    }

    private async Task RetryPendingEntriesIndividuallyAsync(
        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? tx,
        CancellationToken cancellationToken)
    {
        List<RawTelemetry> pendingEntries = Context.ChangeTracker
            .Entries<RawTelemetry>()
            .Where(e => e.State == EntityState.Added)
            .Select(e => e.Entity)
            .ToList();

        foreach (var entry in Context.ChangeTracker.Entries<RawTelemetry>().Where(e => e.State == EntityState.Added).ToList())
        {
            entry.State = EntityState.Detached;
        }

        foreach (RawTelemetry item in pendingEntries)
        {
            bool alreadyExists = await Context.TelemetryRawData
                .AsNoTracking()
                .AnyAsync(x => x.ExternalMessageId == item.ExternalMessageId, cancellationToken);
            if (alreadyExists)
            {
                continue;
            }

            const string ItemSavepoint = "telemetry_item";
            if (tx is not null)
            {
                await tx.CreateSavepointAsync(ItemSavepoint, cancellationToken);
            }

            await Context.TelemetryRawData.AddAsync(item, cancellationToken);

            try
            {
                await Context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                if (tx is not null)
                {
                    await tx.RollbackToSavepointAsync(ItemSavepoint, cancellationToken);
                }

                Context.Entry(item).State = EntityState.Detached;
            }
        }
    }

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is Npgsql.PostgresException { SqlState: Npgsql.PostgresErrorCodes.UniqueViolation };
}
