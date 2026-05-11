using Contracts.Abstractions;

namespace Device.Domain.DomainEvents.ControllerEvents;

public sealed record ControllerNotOnlineDomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid UserId { get; init; }
    public Guid ControllerId { get; init; }
    public DateTime LastSeenAt { get; init; }
}
