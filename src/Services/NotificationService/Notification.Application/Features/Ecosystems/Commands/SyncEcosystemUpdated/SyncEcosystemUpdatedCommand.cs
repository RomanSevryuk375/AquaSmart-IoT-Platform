using Contracts.Abstractions;

namespace Notification.Application.Features.Ecosystems.Commands.SyncEcosystemUpdated;

public sealed record SyncEcosystemUpdatedCommand : ICommand
{
    public Guid EcosystemId { get; init; }
    public Guid UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public Guid ControllerId { get; init; }
    public DateTime CreatedAt { get; init; }
}
