using System.Text.Json.Serialization;

namespace Mobile.Services.Dto.Control;

public record EcosystemDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("type")] int Type, // 1: Aquarium, 2: Terrarium, 3: Growbox, 4: Florarium, 5: Formicary
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("volume")] double? Volume,
    [property: JsonPropertyName("controllerId")] Guid ControllerId,
    [property: JsonPropertyName("createdAt")] DateTime CreatedAt
);
