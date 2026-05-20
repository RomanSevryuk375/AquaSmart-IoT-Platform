namespace Telemetry.Application.DTOs;

public record TelemetryRawChartPointDto
{
    public Guid SensorId { get; init; }
    public double Value { get; init; }
    public DateTime RecordedAt { get; init; }
}
