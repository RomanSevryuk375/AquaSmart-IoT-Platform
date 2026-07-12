using Contracts.Abstractions;

namespace Notification.Application.Features.Users.Commands.SyncUserCreated;

public sealed record SyncUserCreatedCommand : ICommand
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public string TimeZone { get; init; } = string.Empty;
}
