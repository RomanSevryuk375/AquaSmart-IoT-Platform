using System.Text.Json.Serialization;

namespace Mobile.Services.Dto.Identity;

public record LoginRequestDto(
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("password")] string Password
);
