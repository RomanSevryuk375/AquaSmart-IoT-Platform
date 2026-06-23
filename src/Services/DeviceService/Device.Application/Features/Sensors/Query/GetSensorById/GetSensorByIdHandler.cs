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
                id, controller_id, name, connection_protocol, connection_address, 
                type, state, unit, last_value, is_data_delayed, updated_at, created_at
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
                $"Sensor {request.SensorId} not found or access denied."));
        }

        return Result<SensorDto>.Success(sensor);
    }
}
