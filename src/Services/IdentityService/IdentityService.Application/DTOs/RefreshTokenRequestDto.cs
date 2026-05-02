namespace IdentityService.Application.DTOs;

public record RefreshTokenRequestDto
{
    public string RefreshToken { get; init; } = string.Empty;
}
