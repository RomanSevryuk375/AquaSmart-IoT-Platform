namespace Telemetry.Application.DTOs;

public record TelemetryDataFilterDto
{
    public Guid SensorId { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
}
