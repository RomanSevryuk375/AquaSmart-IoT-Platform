using Contracts.Abstractions;

namespace Notification.Application.Features.Users.Commands.SyncUserUpdated;

public sealed record SyncUserUpdatedCommand : ICommand
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
}
