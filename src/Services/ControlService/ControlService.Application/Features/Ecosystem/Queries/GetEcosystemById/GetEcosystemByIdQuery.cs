using Contracts.Abstractions;
using Contracts.Results;

namespace Control.Application.Features.Ecosystem.Queries.GetEcosystemById;

public sealed record GetEcosystemByIdQuery
    : IQuery<Result<EcosystemDto>>, IEcosystemBoundRequest
{
    public Guid EcosystemId { get; init; }
}
