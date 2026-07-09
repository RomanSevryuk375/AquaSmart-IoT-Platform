namespace Device.Domain.Events.SensorEvents;

public sealed record SensorStateChangedDomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid SensorId { get; init; }
    public SensorState State { get; init; }
}
