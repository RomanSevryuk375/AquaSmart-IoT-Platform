using System.Text.Json.Serialization;

namespace Mobile.Services.Dto.Telemetry;

public record TelemetryRawChartResponseDto(
    [property: JsonPropertyName("sensorId")] Guid SensorId,
    [property: JsonPropertyName("dataPoints")] List<TelemetryDataPointDto>? DataPoints
);
