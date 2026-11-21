using System.Text.Json.Serialization;

namespace Mobile.Services.Dto.Identity;

public record RegisterRequestDto(
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("phoneNumber")] string? PhoneNumber,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("password")] string Password,
    [property: JsonPropertyName("timeZone")] string TimeZone = "UTC"
);
