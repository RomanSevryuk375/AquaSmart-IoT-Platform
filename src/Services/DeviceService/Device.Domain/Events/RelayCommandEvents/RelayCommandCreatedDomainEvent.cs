using BuildingBlocks.Domain.Abstractions;

namespace Device.Domain.Events.RelayCommandEvents;

public sealed record RelayCommandCreatedDomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid CommandId { get; init; }
    public Guid ControllerId { get; init; }
    public Guid RelayId { get; init; }
    public bool TargetState { get; init; }
    public DateTime? ExpireAt { get; init; }
    public DateTime CreatedAt { get; init; }
}
