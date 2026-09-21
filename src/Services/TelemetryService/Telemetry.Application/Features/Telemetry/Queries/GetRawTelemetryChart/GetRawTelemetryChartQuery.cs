using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Results;
using Telemetry.Application.DTOs;

namespace Telemetry.Application.Features.Telemetry.Queries.GetRawTelemetryChart;

public sealed record GetRawTelemetryChartQuery
    : IQuery<Result<TelemetryRawChartResponseDto>>
{
    public Guid SensorId { get; init; }
    public Guid UserId { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
    public int Skip { get; init; } = 0;
    public int Take { get; init; } = 100;
}
