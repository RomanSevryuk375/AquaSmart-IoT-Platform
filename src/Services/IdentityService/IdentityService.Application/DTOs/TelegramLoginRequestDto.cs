// Ignore Spelling: Dto

namespace IdentityService.Application.DTOs;

public sealed record TelegramLoginRequestDto
{
    public long ChatId { get; init; }
}
