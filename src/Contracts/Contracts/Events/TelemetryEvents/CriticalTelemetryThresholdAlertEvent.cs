namespace Contracts.Events.TelemetryEvents;

public sealed record CriticalTelemetryThresholdAlertEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid UserId { get; init; }
    public Guid EcosytemId { get; init; }
    public Guid SensorId { get; init; }
    public double Value { get; init; }
    public DateTime RecordedAt { get; init; }
    public Guid RelayId { get; init; }
    public bool RelayState { get; init; }
}
