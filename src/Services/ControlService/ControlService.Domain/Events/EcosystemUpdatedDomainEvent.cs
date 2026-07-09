using Contracts.Abstractions;

namespace Control.Domain.Events;

public sealed record EcosystemUpdatedDomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid EcosystemId { get; init; }
    public Guid UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public Guid ControllerId { get; init; }
    public DateTime CreatedAt { get; init; }
}
