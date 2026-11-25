using System.Data;
using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Results;
using Dapper;
using Device.Application.Features.Controllers.Query.Shared;

namespace Device.Application.Features.Controllers.Query.GetAllControllers;

public sealed class GetAllControllersHandler(ISqlConnectionFactory sqlConnectionFactory)
    : IRequestHandler<GetAllControllersQuery, Result<IReadOnlyList<ControllerDto>>>
{
    public async Task<Result<IReadOnlyList<ControllerDto>>> Handle(
        GetAllControllersQuery request,
        CancellationToken cancellationToken)
    {
        using IDbConnection connection = sqlConnectionFactory.CreateConnection();

        const string Sql = """
            SELECT
                id,
                mac_address,
                name,
                is_online,
                last_seen_at,
                config_version,
                firmware_version,
                ip_address,
                rssi,
                uptime_sec,
                offline_alert_sent
            FROM controllers
            WHERE user_id = @UserId
              AND (@SearchTerm IS NULL OR name ILIKE '%' || @SearchTerm || '%'
                                       OR mac_address ILIKE '%' || @SearchTerm || '%')
              AND (@IsOnline IS NULL OR is_online = @IsOnline)
            ORDER BY created_at DESC
            LIMIT @Take OFFSET @Skip
            """;

        IEnumerable<ControllerDto> controllerList = await connection.QueryAsync<ControllerDto>(Sql,
            new
            {
                request.UserId,
                request.SearchTerm,
                request.IsOnline,
                request.Skip,
                request.Take,
            });

        return Result<IReadOnlyList<ControllerDto>>.Success(controllerList.ToList().AsReadOnly());
    }
}
