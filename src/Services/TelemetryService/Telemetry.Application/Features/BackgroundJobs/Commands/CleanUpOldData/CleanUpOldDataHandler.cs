using System.Data;
using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Domain.Results;
using Dapper;
using MediatR;
using Microsoft.Extensions.Options;
using Telemetry.Application.Extensions;

namespace Telemetry.Application.Features.BackgroundJobs.Commands.CleanUpOldData;

public sealed class CleanUpOldDataHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    IOptions<TelemetrySettings> telemetrySettings) : IRequestHandler<CleanUpOldDataCommand, Result>
{
    private const int BatchSize = 1000;

    public async Task<Result> Handle(CleanUpOldDataCommand request, CancellationToken cancellationToken)
    {
        DateTime rawThreshold = DateTime.UtcNow
            .AddHours(telemetrySettings.Value.MaxLiveTimeForRawDataInHours);

        DateTime minuteThreshold = DateTime.UtcNow
            .AddDays(telemetrySettings.Value.MaxLiveTimeForMinutesDataInDayes);

        DateTime hourlyThreshold = DateTime.UtcNow
            .AddDays(telemetrySettings.Value.MaxLiveTimeForHourseDataInDayes);

        using IDbConnection connection = sqlConnectionFactory.CreateConnection();

        await DeleteOldRawDataAsync(connection, rawThreshold, cancellationToken);
        await DeleteOldAggregateDataAsync(connection, PeriodType.Minute, minuteThreshold, cancellationToken);
        await DeleteOldAggregateDataAsync(connection, PeriodType.Hourly, hourlyThreshold, cancellationToken);

        return Result.Success();
    }

    private static async Task DeleteOldRawDataAsync(
        IDbConnection connection,
        DateTime olderThan,
        CancellationToken cancellationToken)
    {
        const string SQL = """
            WITH cte AS (
                SELECT id
                FROM telemetry_raw_data
                WHERE recorded_at < @OlderThan 
                  AND is_aggregated = true
                LIMIT @BatchSize
            )
            DELETE FROM telemetry_raw_data
            WHERE id IN (SELECT id FROM cte);
            """;

        int rowsDeleted;
        do
        {
            cancellationToken.ThrowIfCancellationRequested();
            rowsDeleted = await connection.ExecuteAsync(new CommandDefinition(
                SQL,
                new { OlderThan = olderThan, BatchSize },
                cancellationToken: cancellationToken));
        } while (rowsDeleted >= BatchSize);
    }

    private static async Task DeleteOldAggregateDataAsync(
        IDbConnection connection,
        PeriodType period,
        DateTime olderThan,
        CancellationToken cancellationToken)
    {
        const string SQL = """
            WITH cte AS (
                SELECT id
                FROM telemetry_aggregate_data
                WHERE created_at < @OlderThan
                  AND is_aggregated = true
                  AND period = @Period
                LIMIT @BatchSize
            )
            DELETE FROM telemetry_aggregate_data
            WHERE id IN (SELECT id FROM cte);
            """;

        int rowsDeleted;
        do
        {
            cancellationToken.ThrowIfCancellationRequested();
            rowsDeleted = await connection.ExecuteAsync(new CommandDefinition(
                SQL,
                new { Period = (int)period, OlderThan = olderThan, BatchSize },
                cancellationToken: cancellationToken));
        } while (rowsDeleted >= BatchSize);
    }
}

