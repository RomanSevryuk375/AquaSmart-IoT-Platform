using BuildingBlocks.Domain.Abstractions;

namespace Device.Domain.Events.ControllerEvents;

public sealed record ControllerConfigChangedDomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid ControllerId { get; init; }
    public string MacAddress { get; init; } = string.Empty;
    public long ConfigVersion { get; init; }
    public string Reason { get; init; } = string.Empty;
}
