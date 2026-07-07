using Contracts.Abstractions;

namespace IdentityService.Domain.Events;

public sealed record UserUpdatedDomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
}
