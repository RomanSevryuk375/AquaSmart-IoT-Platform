using System.Text.Json.Serialization;

namespace Mobile.Services.Dto.Control;

public record RuleConditionRequestDto(
    [property: JsonPropertyName("sensorId")] Guid SensorId,
    [property: JsonPropertyName("condition")] int Condition,
    [property: JsonPropertyName("threshold")] double Threshold,
    [property: JsonPropertyName("hysteresis")] double Hysteresis = 0.0
);

