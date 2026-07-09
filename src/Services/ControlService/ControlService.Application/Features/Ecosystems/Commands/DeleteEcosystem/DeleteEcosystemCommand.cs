using Contracts.Abstractions;
using Control.Application.Interfaces;

namespace Control.Application.Features.Ecosystems.Commands.DeleteEcosystem;

public sealed record DeleteEcosystemCommand
    : ICommand, IEcosystemBoundRequest
{
    public Guid EcosystemId { get; init; }
}
