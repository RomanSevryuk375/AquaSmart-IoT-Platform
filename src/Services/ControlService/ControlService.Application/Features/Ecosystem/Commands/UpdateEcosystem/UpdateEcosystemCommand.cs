using Contracts.Abstractions;

namespace Control.Application.Features.Ecosystem.Commands.UpdateEcosystem;

public sealed record UpdateEcosystemCommand
    : ICommand, IEcosystemBoundRequest
{
    public Guid EcosystemId { get; init; }
    public string Name { get; init; } = string.Empty;
    public double Volume { get; init; }
}
