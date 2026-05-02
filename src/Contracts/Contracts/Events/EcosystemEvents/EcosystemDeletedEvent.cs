namespace Contracts.Events.EcosystemEvents;

public record EcosystemDeletedEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid EcosystemId { get; init; }
}
