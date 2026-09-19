using System.Data;
using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Constants;
using BuildingBlocks.Domain.Results;
using Dapper;
using MediatR;
using Telemetry.Application.DTOs;
using Telemetry.Domain.Entities;

namespace Telemetry.Application.Features.Telemetry.Queries.GetRawTelemetryChart;

internal sealed class GetRawTelemetryChartHandler(ISqlConnectionFactory sqlConnectionFactory)
    : IRequestHandler<GetRawTelemetryChartQuery, Result<TelemetryRawChartResponseDto>>
{
    private const int DefaultPeriodDays = -1;

    public async Task<Result<TelemetryRawChartResponseDto>> Handle(
        GetRawTelemetryChartQuery request,
        CancellationToken cancellationToken)
    {
        DateTime to = request.To ?? DateTime.UtcNow;
        DateTime from = request.From ?? to.AddDays(DefaultPeriodDays);

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
                value AS Value, 
                recorded_at AS RecordedAt
            FROM telemetry_raw_data
            WHERE sensor_id = @SensorId
              AND recorded_at >= @From
              AND recorded_at <= @To
            ORDER BY recorded_at ASC
            LIMIT @Take OFFSET @Skip;
            """;

        using SqlMapper.GridReader multi = await connection.QueryMultipleAsync(SQL, new
        {
            request.SensorId,
            request.UserId,
            From = from,
            To = to,
            request.Take,
            request.Skip
        });

        SensorInfoDto? sensor = await multi.ReadSingleOrDefaultAsync<SensorInfoDto>();
        if (sensor is null)
        {
            return Result<TelemetryRawChartResponseDto>.Failure(Error.NotFound<Sensor>(
                string.Format(ErrorMessages.SensorNotFound, request.SensorId)));
        }

        IEnumerable<TelemetryRawChartPointDto> points = await multi.ReadAsync<TelemetryRawChartPointDto>();

        return Result<TelemetryRawChartResponseDto>.Success(new TelemetryRawChartResponseDto
        {
            SensorId = sensor.Id,
            SensorName = sensor.Name,
            Unit = sensor.Unit,
            Points = points.AsList().AsReadOnly()
        });
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
