using Contracts.Enums;

namespace Telemetry.Application.DTOs;

public record TelemetryAggregateFilterDto
{
    public Guid SensorId { get; init; }
    public PeriodTypeEnum? Period { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
}
