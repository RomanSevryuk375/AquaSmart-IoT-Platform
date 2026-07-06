using Contracts.Abstractions;
using Control.Application.Interfaces;

namespace Control.Application.Features.Ecosystems.Commands.UpdateEcosystem;

public sealed record UpdateEcosystemCommand
    : ICommand, IEcosystemBoundRequest
{
    public Guid EcosystemId { get; init; }
    public string Name { get; init; } = string.Empty;
    public double Volume { get; init; }
}
