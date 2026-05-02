namespace Telemetry.Domain.Entities;

public record TelemetrySummary
{
    public Guid SensorId { get; init; }
    public double MaxValue { get; init; }
    public double MinValue { get; init; }
    public double AvgValue { get; init; }
    public int Count { get; init; }
}
