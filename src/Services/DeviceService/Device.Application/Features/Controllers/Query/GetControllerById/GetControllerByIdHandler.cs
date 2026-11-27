using System.Data;
using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Constants;
using BuildingBlocks.Domain.Results;
using Dapper;
using Device.Application.Features.Controllers.Query.Shared;

namespace Device.Application.Features.Controllers.Query.GetControllerById;

public sealed class GetControllerByIdHandler(ISqlConnectionFactory sqlConnectionFactory)
    : IRequestHandler<GetControllerByIdQuery, Result<ControllerDto>>
{
    public async Task<Result<ControllerDto>> Handle(
        GetControllerByIdQuery request,
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
            WHERE id = @ControllerId
              AND user_id = @UserId
            LIMIT 1
            """;

        ControllerDto? controller = await connection.QueryFirstOrDefaultAsync<ControllerDto>(Sql,
            new { request.ControllerId, request.UserId });
        if (controller is null)
        {
            return Result<ControllerDto>.Failure(Error.NotFound<Controller>(
                string.Format(ErrorMessages.ControllerNotFound, request.ControllerId)));
        }

        return Result<ControllerDto>.Success(controller);
    }
}
