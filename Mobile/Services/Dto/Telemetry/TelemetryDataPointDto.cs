using System.Text.Json.Serialization;

namespace Mobile.Services.Dto.Telemetry;

public record TelemetryDataPointDto(
    [property: JsonPropertyName("value")] double Value,
    [property: JsonPropertyName("timestamp")] DateTime Timestamp
);
