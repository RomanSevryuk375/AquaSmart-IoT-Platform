// Ignore Spelling: Dto

namespace IdentityService.Application.DTOs;

public sealed record TelegramLoginResponseDto
{
    public string AccessToken { get; init; } = string.Empty;
    public int ExpiresHours { get; init; }
}
