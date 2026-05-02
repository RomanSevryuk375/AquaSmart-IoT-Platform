using Contracts.Enums;

namespace Telemetry.Application.DTOs;

public record TelemetryChartPointDto
{
    public double MinValue { get; init; }
    public double MaxValue { get; init; }
    public double AvgValue { get; init; }
    public DateTime Time { get; init; }
}
