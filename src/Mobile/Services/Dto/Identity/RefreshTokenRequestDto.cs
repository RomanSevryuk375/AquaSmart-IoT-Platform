using System.Text.Json.Serialization;

namespace Mobile.Services.Dto.Identity;

public record RefreshTokenRequestDto(
    [property: JsonPropertyName("refreshToken")] string RefreshToken
);
