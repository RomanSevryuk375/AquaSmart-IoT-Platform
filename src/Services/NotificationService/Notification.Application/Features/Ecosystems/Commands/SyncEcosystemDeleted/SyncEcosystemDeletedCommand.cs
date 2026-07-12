using Contracts.Abstractions;

namespace Notification.Application.Features.Ecosystems.Commands.SyncEcosystemDeleted;

public sealed record SyncEcosystemDeletedCommand : ICommand
{
    public Guid EcosystemId { get; init; }
}
