using System.Data;
using Contracts.Results;
using Control.Application.Features.VacationModes.Queries.Shared;
using Control.Domain.Interfaces;
using Dapper;
using MediatR;

namespace Control.Application.Features.VacationModes.Queries.GetAllVacationModes;

internal sealed class GetAllVacationModesHandler(ISqlConnectionFactory sqlConnectionFactory)
    : IRequestHandler<GetAllVacationModesQuery, Result<IReadOnlyList<VacationModeDto>>>
{
    public async Task<Result<IReadOnlyList<VacationModeDto>>> Handle(
        GetAllVacationModesQuery request,
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
            WHERE e.user_id = @UserId
              AND v.ecosystem_id = @EcosystemId
              AND (@IsActive IS NULL OR v.is_active = @IsActive)
            ORDER BY v.created_at DESC
            LIMIT @Take OFFSET @Skip
            """;

        IEnumerable<VacationModeDto> vacations = await connection.QueryAsync<VacationModeDto>(Sql, new
        {
            request.UserId,
            request.EcosystemId,
            request.IsActive,
            request.Take,
            request.Skip
        });

        return Result<IReadOnlyList<VacationModeDto>>.Success(vacations.ToList().AsReadOnly());
    }
}
