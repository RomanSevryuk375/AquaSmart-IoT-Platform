// Ignore Spelling: Dto

namespace IdentityService.Application.DTOs;

public sealed record TelegramLinkTokenResponseDto
{
    public string Link { get; init; } = string.Empty;
    public DateTime ExpireAt { get; init; }
}
