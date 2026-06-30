namespace Contracts.Events.SensorEvents;

public sealed record SensorNoDataAlertEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid UserId { get; init; }
    public Guid EcosytemId { get; init; }
    public Guid SensorId { get; init; }
    public DateTime LastSeenAt { get; init; }
}
