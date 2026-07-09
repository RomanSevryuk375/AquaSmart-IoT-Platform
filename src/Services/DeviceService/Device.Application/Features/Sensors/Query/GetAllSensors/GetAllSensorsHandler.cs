using System.Data;
using Dapper;
using Device.Application.Features.Sensors.Query.Shared;

namespace Device.Application.Features.Sensors.Query.GetAllSensors;

internal sealed class GetAllSensorsHandler(
    ISqlConnectionFactory sqlConnectionFactory)
    : IRequestHandler<GetAllSensorsQuery, Result<IReadOnlyList<SensorDto>>>
{
    public async Task<Result<IReadOnlyList<SensorDto>>> Handle(
        GetAllSensorsQuery request,
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
            WHERE user_id = @UserId
              AND (@ControllerId IS NULL OR controller_id = @ControllerId)
              AND (@Type IS NULL OR type = @Type)
              AND (@State IS NULL OR state = @State)
            ORDER BY created_at DESC
            LIMIT @Take OFFSET @Skip
            """;

        IEnumerable<SensorDto> sensors = await connection.QueryAsync<SensorDto>(SQL,
            new
            {
                request.UserId,
                request.ControllerId,
                request.Type,
                request.State,
                request.Take,
                request.Skip
            });

        return Result<IReadOnlyList<SensorDto>>.Success(sensors.AsList().AsReadOnly());
    }
}
