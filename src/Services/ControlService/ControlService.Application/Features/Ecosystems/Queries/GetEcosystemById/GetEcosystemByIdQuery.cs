using Contracts.Abstractions;
using Contracts.Results;
using Control.Application.Interfaces;

namespace Control.Application.Features.Ecosystems.Queries.GetEcosystemById;

public sealed record GetEcosystemByIdQuery
    : IQuery<Result<EcosystemDto>>, IEcosystemBoundRequest
{
    public Guid EcosystemId { get; init; }
}
