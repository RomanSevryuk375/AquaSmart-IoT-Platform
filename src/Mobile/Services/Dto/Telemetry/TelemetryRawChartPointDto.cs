using System.Text.Json.Serialization;

namespace Mobile.Services.Dto.Telemetry;

public sealed record TelemetryRawChartPointDto
{
    [JsonPropertyName("sensorId")]
    public Guid SensorId { get; init; }

    [JsonPropertyName("value")]
    public double Value { get; init; }

    [JsonPropertyName("recordedAt")]
    public DateTime RecordedAt { get; init; }
}
