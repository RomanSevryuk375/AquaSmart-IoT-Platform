namespace Contracts.Events.EcosystemEvents;

public sealed record EcosystemDeletedEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid EcosystemId { get; init; }
}
