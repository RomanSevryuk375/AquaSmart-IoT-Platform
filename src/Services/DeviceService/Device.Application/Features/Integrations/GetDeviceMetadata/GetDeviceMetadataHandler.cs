using System.Data;
using Dapper;

namespace Device.Application.Features.Integrations.GetDeviceMetadata;

public sealed class GetDeviceMetadataHandler(ISqlConnectionFactory sqlConnectionFactory)
    : IRequestHandler<GetDeviceMetadataQuery, Result<DeviceMetadataDto>>
{
    public async Task<Result<DeviceMetadataDto>> Handle(GetDeviceMetadataQuery request, CancellationToken cancellationToken)
    {
        using IDbConnection connection = sqlConnectionFactory.CreateConnection();

        const string SQL = """
            SELECT 
                (SELECT name
                 FROM sensors
                 WHERE id = @SensorId
                 LIMIT 1) AS SensorName,
                (SELECT name
                 FROM relays
                 WHERE id = @RelayId
                 LIMIT 1) AS RelayName
            """;

        DeviceMetadataDto? metadata = await connection.QuerySingleOrDefaultAsync<DeviceMetadataDto>(SQL, new
        {
            request.SensorId,
            request.RelayId
        });

        if (metadata is null || metadata.SensorName is null || metadata.RelayName is null)
        {
            return Result<DeviceMetadataDto>.Failure(Error.NotFound<DeviceMetadataDto>(
                "Sensor or Relay not found."));
        }

        return Result<DeviceMetadataDto>.Success(metadata);
    }
}
