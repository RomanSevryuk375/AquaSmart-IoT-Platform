using Contracts.Abstractions;

namespace IdentityService.Domain.Events;

public sealed record UserCreatedDomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public string TimeZone { get; init; } = string.Empty;
}
