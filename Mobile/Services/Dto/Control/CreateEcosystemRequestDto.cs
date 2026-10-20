using System.Text.Json.Serialization;

namespace Mobile.Services.Dto.Control;

public record CreateEcosystemRequestDto(
    [property: JsonPropertyName("type")] int Type,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("volume")] double? Volume,
    [property: JsonPropertyName("controllerId")] Guid ControllerId
);
