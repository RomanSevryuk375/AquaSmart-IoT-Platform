namespace IdentityService.Application.DTOs;

public record UserProfileResponseDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public Guid SubscriptionId { get; init; }
    public DateTime CreatedAt { get; init; }
}
