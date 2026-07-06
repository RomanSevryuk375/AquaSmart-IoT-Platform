using System.Data;
using Contracts.Results;
using Control.Application.Features.Schedules.Queries.Shared;
using Control.Domain.Interfaces;
using Dapper;
using MediatR;

namespace Control.Application.Features.Schedules.Queries.GetAllSchedules;

internal sealed class GetAllSchedulesHandler(ISqlConnectionFactory sqlConnectionFactory)
    : IRequestHandler<GetAllSchedulesQuery, Result<IReadOnlyList<ScheduleDto>>>
{
    public async Task<Result<IReadOnlyList<ScheduleDto>>> Handle(
        GetAllSchedulesQuery request,
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
            WHERE e.user_id = @UserId
              AND s.ecosystem_id = @EcosystemId
              AND (@RelayId IS NULL OR s.relay_id = @RelayId)
              AND (@IsEnabled IS NULL OR s.is_enabled = @IsEnabled)
            ORDER BY s.created_at DESC
            LIMIT @Take OFFSET @Skip
            """;

        IEnumerable<ScheduleDto> schedules = await connection.QueryAsync<ScheduleDto>(Sql, new
        {
            request.UserId,
            request.EcosystemId,
            request.RelayId,
            request.IsEnabled,
            request.Take,
            request.Skip
        });

        return Result<IReadOnlyList<ScheduleDto>>.Success(schedules.ToList().AsReadOnly());
    }
}
