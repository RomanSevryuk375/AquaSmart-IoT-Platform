using Contracts.Abstractions;

namespace Device.Domain.Events.RelayEvents;

public sealed record RelayDeletedDomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid RelayId { get; init; }
}
