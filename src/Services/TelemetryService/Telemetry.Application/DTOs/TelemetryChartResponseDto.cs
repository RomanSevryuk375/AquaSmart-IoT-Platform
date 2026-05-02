namespace Telemetry.Application.DTOs;

public record TelemetryChartResponseDto
{
    public Guid SensorId { get; init; }
    public string SensorName { get; init; } = string.Empty;
    public string Unit { get; init; } = string.Empty;
    public IReadOnlyList<TelemetryChartPointDto> Points { get; init; } = [];
}
