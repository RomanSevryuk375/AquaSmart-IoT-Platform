using Contracts.Abstractions;
using Contracts.Results;
using Control.Application.Features.Schedules.Queries.Shared;
using Control.Application.Interfaces;

namespace Control.Application.Features.Schedules.Queries.GetScheduleById;

public sealed record GetScheduleByIdQuery
    : IQuery<Result<ScheduleDto>>, IScheduleBoundRequest
{
    public Guid ScheduleId { get; init; }
    public Guid UserId { get; init; }
}
