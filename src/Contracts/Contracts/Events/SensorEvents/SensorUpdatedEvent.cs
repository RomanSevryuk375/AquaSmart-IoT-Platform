using Contracts.Enums;

namespace Contracts.Events.SensorEvents;

public class SensorUpdatedEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid SensorId { get; init; }
    public Guid ControllerId { get; init; }
    public string Name { get; init; } = string.Empty;
    public SensorTypeEnum Type { get; init; }
    public SensorStateEnum State { get; init; }
    public string Unit { get; init; } = string.Empty;
    public DateTime UpdatedAt { get; init; }
    public DateTime CreatedAt { get; init; }
}
