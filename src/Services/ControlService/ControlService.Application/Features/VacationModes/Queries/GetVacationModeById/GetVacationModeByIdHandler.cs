using System.Data;
using Contracts.Results;
using Control.Application.Features.VacationModes.Queries.Shared;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using Dapper;
using MediatR;

namespace Control.Application.Features.VacationModes.Queries.GetVacationModeById;

internal sealed class GetVacationModeByIdHandler(ISqlConnectionFactory sqlConnectionFactory)
    : IRequestHandler<GetVacationModeByIdQuery, Result<VacationModeDto>>
{
    public async Task<Result<VacationModeDto>> Handle(
        GetVacationModeByIdQuery request,
        CancellationToken cancellationToken)
    {
        using IDbConnection connection = sqlConnectionFactory.CreateConnection();

        const string Sql = """
            SELECT
              v.id,
              v.ecosystem_id,
              v.date_range,
              v.calculated_feed,
              v.is_active,
              v.created_at
            FROM vacations v
            JOIN ecosystems e ON v.ecosystem_id = e.id
            WHERE v.id = @VacationModeId
            AND e.user_id = @UserId
            """;

        VacationModeDto? vacation = await connection.QuerySingleOrDefaultAsync<VacationModeDto>(Sql, new
        {
            request.VacationModeId,
            request.UserId
        });

        if (vacation is null)
        {
            return Result<VacationModeDto>.Failure(Error.NotFound<VacationMode>(
                $"VacationMode {request.VacationModeId} not found."));
        }

        return Result<VacationModeDto>.Success(vacation);
    }
}
