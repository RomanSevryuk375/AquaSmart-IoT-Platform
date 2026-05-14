namespace Contracts.Events.EcosystemEvents;

public sealed record EcosystemCreatedEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid EcosystemId { get; init; }
    public string Name { get; init; } = string.Empty;
    public Guid UserId { get; init; }
    public Guid ControllerId { get; init; }
}
