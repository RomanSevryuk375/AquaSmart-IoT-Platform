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
                (SELECT name FROM controllers WHERE id = @ControllerId) AS ControllerName,
                (SELECT name FROM sensors WHERE id = @SensorId) AS SensorName,
                (SELECT name FROM relays WHERE id = @RelayId) AS RelayName
            """;

        DeviceMetadataDto? metadata = await connection.QuerySingleOrDefaultAsync<DeviceMetadataDto>(SQL, new
        {
            ControllerId = request.ControllerId ?? Guid.Empty,
            SensorId = request.SensorId ?? Guid.Empty,
            RelayId = request.RelayId ?? Guid.Empty
        });

        if (metadata is null ||
           (request.ControllerId.HasValue && metadata.ControllerName is null) ||
           (request.SensorId.HasValue && metadata.SensorName is null) ||
           (request.RelayId.HasValue && metadata.RelayName is null))
        {
            return Result<DeviceMetadataDto>.Failure(Error.NotFound(
                "Hardware.NotFound", "One or more hardware components were not found in DeviceService."));
        }

        return Result<DeviceMetadataDto>.Success(metadata);
    }
}
