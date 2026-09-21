using System.Data;
using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Constants;
using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Domain.Results;
using Dapper;
using MediatR;
using Telemetry.Application.DTOs;
using Telemetry.Domain.Entities;

namespace Telemetry.Application.Features.Telemetry.Queries.GetAggregatedTelemetryChart;

internal sealed class GetAggregatedTelemetryChartHandler(ISqlConnectionFactory sqlConnectionFactory)
    : IRequestHandler<GetAggregatedTelemetryChartQuery, Result<TelemetryChartResponseDto>>
{
    private const int MaxHoursForMinuteInterval = 6;
    private const int MaxDaysForHourlyInterval = 7;

    public async Task<Result<TelemetryChartResponseDto>> Handle(
        GetAggregatedTelemetryChartQuery request,
        CancellationToken cancellationToken)
    {
        DateTime to = request.To ?? DateTime.UtcNow;
        DateTime from = request.From ?? to.AddDays(-1);
        PeriodType period = request.Period ?? DetermineBestPeriod(from, to);

        using IDbConnection connection = sqlConnectionFactory.CreateConnection();

        const string SQL = """
            SELECT s.id AS Id, s.name AS Name, s.unit AS Unit
            FROM sensors s
            JOIN ecosystems e ON s.ecosystem_id = e.id
            WHERE s.id = @SensorId
              AND e.user_id = @UserId
            LIMIT 1;

            SELECT 
                sensor_id AS SensorId,
                summary_min_value AS MinValue,
                summary_max_value AS MaxValue,
                summary_avg_value AS AvgValue,
                period_start AS Time
            FROM telemetry_aggregate_data
            WHERE sensor_id = @SensorId
              AND period = @Period
              AND period_start >= @From
              AND period_start <= @To
            ORDER BY period_start ASC
            LIMIT @Take OFFSET @Skip;
            """;

        using SqlMapper.GridReader multi = await connection.QueryMultipleAsync(SQL, new
        {
            request.SensorId,
            request.UserId,
            Period = (int)period,
            From = from,
            To = to,
            request.Take,
            request.Skip
        });

        SensorInfoDto? sensor = await multi.ReadSingleOrDefaultAsync<SensorInfoDto>();
        if (sensor is null)
        {
            return Result<TelemetryChartResponseDto>.Failure(Error.NotFound<Sensor>(
                string.Format(ErrorMessages.SensorNotFound, request.SensorId)));
        }

        IEnumerable<TelemetryChartPointDto> points = await multi.ReadAsync<TelemetryChartPointDto>();

        return Result<TelemetryChartResponseDto>.Success(new TelemetryChartResponseDto
        {
            SensorId = sensor.Id,
            SensorName = sensor.Name,
            Unit = sensor.Unit,
            Points = points.AsList().AsReadOnly()
        });
    }

    private static PeriodType DetermineBestPeriod(DateTime from, DateTime to)
    {
        TimeSpan duration = to - from;

        if (duration.TotalHours <= MaxHoursForMinuteInterval)
        {
            return PeriodType.Minute;
        }

        if (duration.TotalDays <= MaxDaysForHourlyInterval)
        {
            return PeriodType.Hourly;
        }

        return PeriodType.Daily;
    }
    private sealed record SensorInfoDto
    {
#pragma warning disable S3459 
        public Guid Id { get; }
#pragma warning restore S3459 
        public string Name { get; init; } = string.Empty;
        public string Unit { get; init; } = string.Empty;
    }
}
