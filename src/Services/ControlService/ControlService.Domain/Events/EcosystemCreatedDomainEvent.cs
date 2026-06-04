using Contracts.Abstractions;

namespace Control.Domain.Events;

public sealed record EcosystemCreatedDomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid EcosystemId { get; init; }
    public string Name { get; init; } = string.Empty;
    public Guid UserId { get; init; }
    public Guid ControllerId { get; init; }
}
