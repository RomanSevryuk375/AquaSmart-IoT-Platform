namespace Contracts.Events.RelayEvents;

public record RelayModeChangedEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid RelayId { get; init; }
    public bool IsManual { get; init; }
}
