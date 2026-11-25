namespace BuildingBlocks.IntegrationEvents.Events.Relays;

public sealed record RelayModeSyncedEvent : BaseIntegrationEvent
{
    public Guid ControllerId { get; init; }
    public Guid RelayId { get; init; }
    public bool IsManual { get; init; }
    public DateTime SyncedAt { get; init; }
}
