using System.Text.Json.Serialization;

namespace Mobile.Services.Dto.Identity;

public record LoginResponseDto(
    [property: JsonPropertyName("accessToken")] string AccessToken,
    [property: JsonPropertyName("refreshToken")] string RefreshToken
);
