using System.Text.Json.Serialization;

namespace Mobile.Services.Dto.Device;

public record SensorCreatedResponse(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("controllerId")] Guid ControllerId,
    [property: JsonPropertyName("name")] string Name
);

