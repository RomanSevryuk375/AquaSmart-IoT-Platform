namespace Device.Domain.Events.RelayEvents;

public sealed record RelayUpdatedDomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid RelayId { get; init; }
    public Guid ControllerId { get; init; }
    public Guid? PowerSensorId { get; init; }
    public string DeviceName { get; init; } = null!;
    public RelayPurposeEnum Purpose { get; init; }
    public bool IsManual { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}
