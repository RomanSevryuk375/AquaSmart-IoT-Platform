namespace Telemetry.Application.DTOs;

public record TelemetryRawChartResponseDto
{
    public Guid SensorId { get; init; }
    public string SensorName { get; init; } = string.Empty;
    public string Unit { get; init; } = string.Empty;
    public IReadOnlyList<TelemetryRawChartPointDto> Points { get; init; } = [];
}
