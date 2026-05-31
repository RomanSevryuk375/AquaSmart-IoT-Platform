using Contracts.Results;
using MediatR;

namespace Control.Application.CQRS.Ecosystem.Commands.DeleteEcosystem;

public sealed record DeleteEcosystemCommand : IRequest<Result>
{
    public Guid EcosystemId { get; init; }
}
