namespace Contracts.Events.SensorEvents;

public record SensorRenamedEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid SensorId { get; init; }
    public string Name { get; init; } = string.Empty;
}
