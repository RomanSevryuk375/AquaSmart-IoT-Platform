using System.Text.Json.Serialization;

namespace Mobile.Services.Dto.Identity;

public record UpdateProfileRequestDto(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("phoneNumber")] string? PhoneNumber
);
