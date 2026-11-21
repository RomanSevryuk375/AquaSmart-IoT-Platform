using System.Text.Json.Serialization;

namespace Mobile.Services.Dto.Control;

public record UpdateEcosystemRequestDto(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("volume")] double? Volume,
    [property: JsonPropertyName("controllerId")] Guid ControllerId
);
