using Contracts.Abstractions;
using Contracts.Enums;

namespace Device.Domain.DomainEvents.SensorEvents;

public sealed record SensorStateChangedDomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid SensorId { get; init; }
    public SensorStateEnum State { get; init; }
}
