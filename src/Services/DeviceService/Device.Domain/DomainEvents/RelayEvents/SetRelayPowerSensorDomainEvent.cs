using Contracts.Abstractions;

namespace Device.Domain.DomainEvents.RelayEvents;

public sealed record SetRelayPowerSensorDomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid RelayId { get; init; }
    public Guid PowerSensorId { get; init; }
}
