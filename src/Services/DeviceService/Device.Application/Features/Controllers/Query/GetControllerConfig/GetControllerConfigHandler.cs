using System.Data;
using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Results;
using Dapper;
using Device.Application.Extesions;
using Device.Domain.ValueObjects;
using Microsoft.Extensions.Options;

namespace Device.Application.Features.Controllers.Query.GetControllerConfig;

public sealed class GetControllerConfigHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    IOptions<DeviceSettings> deviceOptions)
    : IRequestHandler<GetControllerConfigQuery, Result<ControllerConfig>>
{
    public async Task<Result<ControllerConfig>> Handle(
        GetControllerConfigQuery request,
        CancellationToken cancellationToken)
    {
        Result<MacAddress> macResult = MacAddress.Create(request.MacAddress);
        string normalizedMac = macResult.IsSuccess ? macResult.Value.Value : request.MacAddress;

        using IDbConnection connection = sqlConnectionFactory.CreateConnection();

        const string Sql = """
            SELECT config_version
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

        await using SqlMapper.GridReader multi = await connection.QueryMultipleAsync(
            Sql, new { MacAddress = normalizedMac });

        long? configVersion = await multi.ReadFirstOrDefaultAsync<long?>();
        IEnumerable<RelayConfig> relayConfig = await multi.ReadAsync<RelayConfig>();
        IEnumerable<SensorConfig> sensorConfig = await multi.ReadAsync<SensorConfig>();

        var controllerConfig = new ControllerConfig
        {
            ConfigVersion = configVersion ?? 1,
            SendIntervalMs = deviceOptions.Value.DefaultSendIntervalMs,
            MaxBatchSize = deviceOptions.Value.MaxConfigBatchSize,
            Sensors = sensorConfig.AsList(),
            Relays = relayConfig.AsList(),
        };

        return Result<ControllerConfig>.Success(controllerConfig);
    }
}
