using Contracts.Abstractions;

namespace Control.Application.Features.Ecosystem.Commands.DeleteEcosystem;

public sealed record DeleteEcosystemCommand
    : ICommand, IEcosystemBoundRequest
{
    public Guid EcosystemId { get; init; }
}
