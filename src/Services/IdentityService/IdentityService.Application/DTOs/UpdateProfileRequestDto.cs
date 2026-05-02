namespace IdentityService.Application.DTOs;

public record UpdateProfileRequestDto
{
    public string Name { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
}
