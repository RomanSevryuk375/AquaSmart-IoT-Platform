using Contracts.Abstractions;
using Contracts.Results;
using MediatR;

namespace Control.Application.CQRS.Ecosystem.Queries.GetEcosystemById;

public sealed record GetEcosystemByIdQuery 
    : IRequest<Result<EcosystemDto>>, IEcosystemBoundRequest
{
    public Guid EcosystemId { get; init; }
}
