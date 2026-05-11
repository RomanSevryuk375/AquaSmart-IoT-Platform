using Contracts.Abstractions;
using Contracts.Enums;

namespace Device.Domain.DomainEvents.RelayEvents;

public sealed record RelayCreatedDomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid RelayId { get; init; }
    public Guid ControllerId { get; init; }
    public Guid? PowerSensorId { get; init; }
    public string Name { get; init; } = string.Empty;
    public RelayPurposeEnum Purpose { get; init; }
    public bool IsManual { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}
