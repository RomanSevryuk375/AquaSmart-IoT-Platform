using Contracts.Enums;

namespace Contracts.Events.SensorEvents;

public sealed record SensorNoDataEvent 
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid SensorId { get; init; } 
    public SensorStateEnum State { get; init; }
    public DateTime LastSeenAt { get; init; }
}
