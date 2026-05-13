using Contracts.Abstractions;
using Contracts.Enums;

namespace Control.Domain.Events;

public sealed record RelayStateChangedDomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid ControllerId { get; init; }
    public Guid RelayId { get; init; }
    public RuleActionEnum Action { get; init; }
    public DateTime? ExpireAt { get; init; }
}
