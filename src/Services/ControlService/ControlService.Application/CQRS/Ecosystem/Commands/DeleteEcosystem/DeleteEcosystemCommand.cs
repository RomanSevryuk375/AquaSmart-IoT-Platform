using Contracts.Abstractions;

namespace Control.Application.CQRS.Ecosystem.Commands.DeleteEcosystem;

public sealed record DeleteEcosystemCommand 
    : ICommand, IEcosystemBoundRequest
{
    public Guid EcosystemId { get; init; }
}
