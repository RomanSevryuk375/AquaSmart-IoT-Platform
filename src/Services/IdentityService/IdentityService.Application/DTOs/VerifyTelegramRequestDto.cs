// Ignore Spelling: Dto

namespace IdentityService.Application.DTOs;

public sealed record VerifyTelegramRequestDto
{
    public string Token { get; init; } = string.Empty;
    public long ChatId { get; init; }
}
