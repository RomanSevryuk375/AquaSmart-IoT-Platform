using Contracts.Abstractions;

namespace Control.Domain.Events;

public sealed record EcosystemDeletedDomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid EcosystemId { get; init; }
}
