namespace BuildingBlocks.IntegrationEvents.Events.Relays;

public sealed record RemoveRelayPowerSensorEvent : BaseIntegrationEvent
{
    public Guid RelayId { get; init; }
    public Guid PowerSensorId { get; init; }
}
