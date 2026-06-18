using Contracts.Enums;

namespace Contracts.Events.SensorEvents;

public sealed record SensorCreatedEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid SensorId { get; init; }
    public Guid ControllerId { get; init; }
    public string Name { get; init; } = string.Empty;
    public SensorType Type { get; init; }
    public SensorState State { get; init; }
    public string Unit { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
