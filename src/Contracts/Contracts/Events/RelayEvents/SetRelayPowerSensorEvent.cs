namespace Contracts.Events.RelayEvents;

public sealed record SetRelayPowerSensorEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid RelayId { get; init; }
    public Guid PowerSensorId { get; init; }
}
