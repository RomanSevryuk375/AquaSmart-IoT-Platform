namespace BuildingBlocks.IntegrationEvents.Events.Telemetrys;

public sealed record TelemetryBatchReceivedFromDeviceEvent : BaseIntegrationEvent
{
    public Guid ControllerId { get; init; }
    public Guid UserId { get; init; }
    public string MacAddress { get; init; } = string.Empty;
    public IReadOnlyList<TelemetryBatchEventItem> Items { get; init; } = [];
}
