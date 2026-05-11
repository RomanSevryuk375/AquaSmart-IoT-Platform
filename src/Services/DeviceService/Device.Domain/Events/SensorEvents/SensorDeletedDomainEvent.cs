using Contracts.Abstractions;

namespace Device.Domain.DomainEvents.SensorEvents;

public sealed record SensorDeletedDomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid SensorId { get; init; }
}
