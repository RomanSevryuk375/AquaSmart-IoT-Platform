using Contracts.Abstractions;
using Contracts.Enums;

namespace Telemetry.Domain.Events;

public sealed record SensorNoDataDomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid SensorId { get; init; }
    public SensorState State { get; init; }
    public DateTime LastSeenAt { get; init; }
}
