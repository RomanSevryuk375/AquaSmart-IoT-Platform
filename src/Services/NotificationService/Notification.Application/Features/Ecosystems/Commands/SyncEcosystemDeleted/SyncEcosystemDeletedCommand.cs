using Contracts.Abstractions;

namespace Notification.Application.Features.Ecosystems.Commands.SyncEcosystemDeleted;

public sealed record SyncEcosystemUpdatedCommand : ICommand
{
    public Guid EcosystemId { get; init; }
}
