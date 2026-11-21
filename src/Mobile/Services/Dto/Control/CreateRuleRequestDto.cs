using System.Text.Json.Serialization;

namespace Mobile.Services.Dto.Control;

public record CreateRuleRequestDto(
    [property: JsonPropertyName("ecosystemId")] Guid EcosystemId,
    [property: JsonPropertyName("relayId")] Guid RelayId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("operator")] int Operator,
    [property: JsonPropertyName("action")] int Action,
    [property: JsonPropertyName("isActive")] bool IsActive,
    [property: JsonPropertyName("conditions")] List<RuleConditionRequestDto> Conditions
);

