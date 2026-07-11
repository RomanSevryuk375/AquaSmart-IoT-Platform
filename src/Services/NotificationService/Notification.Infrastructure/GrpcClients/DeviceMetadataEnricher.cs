// Ignore Spelling: Enricher

using Contracts.gRPC.Devices;
using Contracts.Results;
using Grpc.Core;
using Notification.Application.DTOs;
using Notification.Application.Interfaces;

namespace Notification.Infrastructure.GrpcClients;

public sealed class DeviceMetadataEnricher(DeviceIntegrationGrpc.DeviceIntegrationGrpcClient client)
    : IDeviceMetadataEnricher
{
    public async Task<Result<DeviceMetadataDto>> EnrichAsync(
        Guid? controllerId,
        Guid? sensorId,
        Guid? relayId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetMetadataRequest
            {
                ControllerId = controllerId?.ToString() ?? string.Empty,
                SensorId = sensorId?.ToString() ?? string.Empty,
                RelayId = relayId?.ToString() ?? string.Empty,
            };

            GetMetadataResponse response = await client.GetMetadataAsync(
                request, cancellationToken: cancellationToken);

            return Result<DeviceMetadataDto>.Success(new DeviceMetadataDto
            {
                ControllerName = response.ControllerName,
                RelayName = response.RelayName,
                SensorName = response.SensorName,
            });
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return Result<DeviceMetadataDto>.Failure(Error.NotFound("Device.NotFound",
                $"Sensor {sensorId} or Relay {relayId} or Controller {controllerId} does not exist."));
        }
        catch (RpcException ex)
        {
            return Result<DeviceMetadataDto>.Failure(Error.Failure("Grpc.Error",
                $"Communication failed: {ex.Status.Detail}"));
        }
    }
}
