namespace Contracts.Events.UserEvents;

public record SubscriptionDowngradedEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid UserId { get; init; }
    public Guid NewSubscriptionId { get; init; }
}
