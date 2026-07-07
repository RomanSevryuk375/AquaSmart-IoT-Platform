// Ignore Spelling: Dto

namespace IdentityService.Application.DTOs;

public sealed record ChangePasswordRequestDto
{
    public string CurrentPassword { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}
