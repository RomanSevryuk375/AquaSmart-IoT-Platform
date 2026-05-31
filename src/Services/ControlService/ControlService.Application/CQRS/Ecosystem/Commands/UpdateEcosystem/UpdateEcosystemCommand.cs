using Contracts.Results;
using MediatR;

namespace Control.Application.CQRS.Ecosystem.Commands.UpdateEcosystem;

public sealed record UpdateEcosystemCommand : IRequest<Result>
{
    public Guid EcosystemId { get; init; }
    public string Name { get; init; } = string.Empty;
    public double Volume { get; init; }
}
