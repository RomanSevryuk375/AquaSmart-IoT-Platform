using System.Data;
using Dapper;
using Device.Application.Extesions;
using Device.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace Device.Application.Features.Controllers.Query.GetControllerConfig;

internal sealed class GetControllerConfigHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    IOptions<DeviceSettings> deviceOptions,
    IMyHasher myHasher)
    : IRequestHandler<GetControllerConfigQuery, Result<ControllerConfig>>
{
    public async Task<Result<ControllerConfig>> Handle(
        GetControllerConfigQuery request,
        CancellationToken cancellationToken)
    {
        using IDbConnection connection = sqlConnectionFactory.CreateConnection();

        const string SQL = """
            SELECT id AS Id, device_token_hash AS DeviceTokenHash
            FROM controllers
            WHERE mac_address = @MacAddress
            LIMIT 1;

            SELECT
                id AS relay_id, 
                name, 
                split_part(connection_address, '_', 1) AS connection_protocol, 
                split_part(connection_address, '_', 2) AS connection_address,
                is_normally_open, purpose, is_active, is_manual
            FROM relays
            WHERE controller_id = (SELECT id FROM controllers WHERE mac_address = @MacAddress LIMIT 1);

            SELECT
                id AS sensor_id, 
                name, 
                split_part(connection_address, '_', 1) AS connection_protocol, 
                split_part(connection_address, '_', 2) AS connection_address, 
                type, unit
            FROM sensors
            WHERE controller_id = (SELECT id FROM controllers WHERE mac_address = @MacAddress LIMIT 1);
            """;

        using SqlMapper.GridReader multi = await connection.QueryMultipleAsync(SQL, new { request.MacAddress });

        ControllerAuthInfo? controllerAuth = await multi.ReadSingleOrDefaultAsync<ControllerAuthInfo>();
        if (controllerAuth is null ||
            !myHasher.Verify(request.DeviceToken, controllerAuth.DeviceTokenHash))
        {
            return Result<ControllerConfig>.Failure(Error.NotFound<Controller>(
                ErrorMessages.InvalidCredentials));
        }

        IEnumerable<RelayConfig> relayConfig = await multi.ReadAsync<RelayConfig>();
        IEnumerable<SensorConfig> sensorConfig = await multi.ReadAsync<SensorConfig>();

        var controllerConfig = new ControllerConfig
        {
            SendIntervalMs = deviceOptions.Value.DefaultSendIntervalMs,
            MaxBatchSize = deviceOptions.Value.MaxConfigBatchSize,
            Sensors = sensorConfig.AsList(),
            Relays = relayConfig.AsList(),
        };

        return Result<ControllerConfig>.Success(controllerConfig);
    }
}
