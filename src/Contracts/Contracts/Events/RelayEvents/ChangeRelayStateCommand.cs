using Contracts.Enums;

namespace Contracts.Events.RelayEvents;

public record ChangeRelayStateCommand
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid ControllerId { get; init; }
    public Guid RelayId { get; init; }
    public RuleActionEnum Action { get; init; }
    public DateTime? ExpireAt { get; init; }
}
