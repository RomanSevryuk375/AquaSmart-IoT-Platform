namespace Device.Domain.DomainEvents.SensorEvents;

public sealed record SensorCreatedDomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid SensorId { get; init; }
    public Guid ControllerId { get; init; }
    public string Name { get; init; } = string.Empty;
    public SensorType Type { get; init; }
    public SensorState State { get; init; }
    public string Unit { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
