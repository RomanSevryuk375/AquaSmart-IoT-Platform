namespace Contracts.Events.TelemetryEvents;

public sealed record TelemetryBatchEventItem
{
    public Guid SensorId { get; init; }
    public double Value { get; init; }
    public string ExternalMessageId { get; init; } = string.Empty;
    public DateTime RecordedAt { get; init; }
}
