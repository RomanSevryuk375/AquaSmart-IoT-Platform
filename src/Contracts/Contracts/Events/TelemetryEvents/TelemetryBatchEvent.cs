namespace Contracts.Events.TelemetryEvents;

public record TelemetryBatchEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid ControllerId { get; init; }
    public List<TelemetryBatchEventItem> Items { get; init; } = [];
}
