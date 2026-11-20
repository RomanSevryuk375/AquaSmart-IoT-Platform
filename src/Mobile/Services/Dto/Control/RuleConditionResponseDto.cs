using System.Text.Json.Serialization;

namespace Mobile.Services.Dto.Control;

public record RuleConditionResponseDto(
    [property: JsonPropertyName("sensorId")] Guid SensorId,
    [property: JsonPropertyName("condition")] int Condition,
    [property: JsonPropertyName("threshold")] double Threshold
);
