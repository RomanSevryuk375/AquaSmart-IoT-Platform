using System.Data;
using Contracts.Results;
using Control.Application.Features.Schedules.Queries.Shared;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using Dapper;
using MediatR;

namespace Control.Application.Features.Schedules.Queries.GetScheduleById;

internal sealed class GetScheduleByIdHandler(ISqlConnectionFactory sqlConnectionFactory)
    : IRequestHandler<GetScheduleByIdQuery, Result<ScheduleDto>>
{
    public async Task<Result<ScheduleDto>> Handle(
        GetScheduleByIdQuery request,
        CancellationToken cancellationToken)
    {
        using IDbConnection connection = sqlConnectionFactory.CreateConnection();

        const string Sql = """
            SELECT
              s.id,
              s.ecosystem_id,
              s.relay_id,
              s.cron_expression,
              s.duration_min,
              s.is_enabled,
              s.is_fade_mode,
              s.created_at
            FROM schedules s
            JOIN ecosystems e ON s.ecosystem_id = e.id
            WHERE s.id = @ScheduleId
            AND e.user_id = @UserId
            """;

        ScheduleDto? schedule = await connection.QuerySingleOrDefaultAsync<ScheduleDto>(Sql, new
        {
            request.ScheduleId,
            request.UserId
        });

        if (schedule is null)
        {
            return Result<ScheduleDto>.Failure(Error.NotFound<Schedule>(
                $"Schedule {request.ScheduleId} not found."));
        }

        return Result<ScheduleDto>.Success(schedule);
    }
}
