using System.Text.Json.Serialization;

namespace Mobile.Services.Dto.Device;

public record ControllerDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("macAddress")] string MacAddress,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("isOnline")] bool IsOnline,
    [property: JsonPropertyName("lastSeenAt")] DateTime LastSeenAt
);
