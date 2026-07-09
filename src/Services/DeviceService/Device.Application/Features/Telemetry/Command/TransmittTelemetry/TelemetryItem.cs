namespace Device.Application.Features.Telemetry.Command.TransmittTelemetry;

public sealed record TelemetryItem
{
    public Guid SensorId { get; init; }
    public double Value { get; init; }
    public string ExternalMessageId { get; init; } = string.Empty;
    public DateTime RecordedAt { get; init; }
}
