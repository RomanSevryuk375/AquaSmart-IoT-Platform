// Ignore Spelling: Dto

namespace IdentityService.Application.DTOs;

public class RegisterUserRequestDto
{
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string TimeZone { get; init; } = string.Empty;
}
