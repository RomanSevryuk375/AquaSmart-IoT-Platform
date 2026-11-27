namespace BuildingBlocks.IntegrationEvents.Events.Relays;

public sealed record RelayStateSyncedEvent : BaseIntegrationEvent
{
    public Guid ControllerId { get; init; }
    public Guid RelayId { get; init; }
    public bool IsActive { get; init; }
    public DateTime SyncedAt { get; init; }
}
