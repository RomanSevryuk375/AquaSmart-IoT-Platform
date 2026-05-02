namespace Telemetry.Domain.SpecificationParams;

public sealed record TelemetryFilterParams
{
    public Guid? SensorId { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
}
