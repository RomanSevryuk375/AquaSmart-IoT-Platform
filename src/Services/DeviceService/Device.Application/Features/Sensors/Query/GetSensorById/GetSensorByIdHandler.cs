using System.Data;
using Dapper;
using Device.Application.Features.Sensors.Query.Shared;

namespace Device.Application.Features.Sensors.Query.GetSensorById;

internal sealed class GetSensorByIdHandler(
    ISqlConnectionFactory sqlConnectionFactory)
    : IRequestHandler<GetSensorByIdQuery, Result<SensorDto>>
{
    public async Task<Result<SensorDto>> Handle(
        GetSensorByIdQuery request,
        CancellationToken cancellationToken)
    {
        using IDbConnection connection = sqlConnectionFactory.CreateConnection();

        const string SQL = """
            SELECT 
                id, controller_id, name,
                split_part(connection_address, '_', 1) AS ConnectionProtocol, 
                split_part(connection_address, '_', 2) AS ConnectionAddress, 
                type, state, unit, created_at
            FROM sensors
            WHERE id = @SensorId
              AND user_id = @UserId
            LIMIT 1
            """;

        SensorDto? sensor = await connection.QueryFirstOrDefaultAsync<SensorDto>(SQL,
            new { request.SensorId, request.UserId });
        if (sensor is null)
        {
            return Result<SensorDto>.Failure(Error.NotFound<Sensor>(
                string.Format(ErrorMessages.SensorNotFoundOrAccessDenied, request.SensorId)));
        }

        return Result<SensorDto>.Success(sensor);
    }
}
