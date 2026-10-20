using System.Text.Json.Serialization;

namespace Mobile.Services.Dto.Device;

public record AddControllerRequestDto(
    [property: JsonPropertyName("macAddress")] string MacAddress,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("isOnline")] bool IsOnline = true
);
