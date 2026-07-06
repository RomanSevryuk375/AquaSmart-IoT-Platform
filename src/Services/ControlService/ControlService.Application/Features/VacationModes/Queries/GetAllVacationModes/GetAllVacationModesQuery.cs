using Contracts.Abstractions;
using Contracts.Results;
using Control.Application.Features.VacationModes.Queries.Shared;
using Control.Application.Interfaces;

namespace Control.Application.Features.VacationModes.Queries.GetAllVacationModes;

public sealed record GetAllVacationModesQuery
    : IQuery<Result<IReadOnlyList<VacationModeDto>>>, IEcosystemBoundRequest
{
    public Guid EcosystemId { get; init; }
    public Guid UserId { get; init; }
    public bool? IsActive { get; init; }
    public int Skip { get; init; } = 0;
    public int Take { get; init; } = 10;
}
