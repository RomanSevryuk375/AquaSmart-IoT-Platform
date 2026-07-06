using Contracts.Abstractions;
using Contracts.Results;
using Control.Application.Features.Schedules.Queries.Shared;
using Control.Application.Interfaces;

namespace Control.Application.Features.Schedules.Queries.GetAllSchedules;

public sealed record GetAllSchedulesQuery
    : IQuery<Result<IReadOnlyList<ScheduleDto>>>, IEcosystemBoundRequest
{
    public Guid EcosystemId { get; init; }
    public Guid UserId { get; init; }
    public Guid? RelayId { get; init; }
    public bool? IsEnabled { get; init; }
    public int Skip { get; init; } = 0;
    public int Take { get; init; } = 10;
}
