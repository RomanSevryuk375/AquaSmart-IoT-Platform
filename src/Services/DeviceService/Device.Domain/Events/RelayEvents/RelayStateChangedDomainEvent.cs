namespace Device.Domain.Events.RelayEvents;

public sealed record RelayStateChangedDomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid ControllerId { get; init; }
    public Guid RelayId { get; init; }
    public bool TargetState { get; init; }
    public DateTime? ExpireAt { get; init; }
}
