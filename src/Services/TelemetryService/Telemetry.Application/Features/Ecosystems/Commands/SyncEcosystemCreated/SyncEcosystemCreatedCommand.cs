using Contracts.Abstractions;

namespace Telemetry.Application.Features.Ecosystems.Commands.SyncEcosystemCreated;

public sealed record SyncEcosystemCreatedCommand : ICommand
{
    public Guid EcosystemId { get; init; }
    public string Name { get; init; } = string.Empty;
    public Guid UserId { get; init; }
    public Guid ControllerId { get; init; }
}
