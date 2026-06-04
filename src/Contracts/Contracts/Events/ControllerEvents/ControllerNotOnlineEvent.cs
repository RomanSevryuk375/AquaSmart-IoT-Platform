namespace Contracts.Events.ControllerEvents;

public sealed record ControllerNotOnlineEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid UserId { get; init; }
    public Guid ControllerId { get; init; }
    public DateTime LastSeenAt { get; init; }
}
