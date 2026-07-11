// Ignore Spelling: Validator

using Contracts.gRPC.Devices;
using Contracts.Results;
using Control.Domain.Interfaces;
using Grpc.Core;

namespace Control.Infrastructure.GrpcClients;

public sealed class HardwareValidator(DeviceIntegrationGrpc.DeviceIntegrationGrpcClient client)
    : IHardwareValidator
{
    public async Task<Result> ValidateAssignmentAsync(
        Guid sensorId,
        Guid relayId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new ValidateHardwareAssignmentRequest
            {
                SensorId = sensorId.ToString(),
                RelayId = relayId.ToString()
            };

            ValidateHardwareAssignmentResponse response = await client.ValidateHardwareAssignmentAsync(
                request, cancellationToken: cancellationToken);
            if (!response.IsValid)
            {
                return Result.Failure(Error.Conflict("Hardware.Mismatch",
                    $"Sensor {sensorId} and Relay {relayId} do not belong to the same controller."));
            }

            return Result.Success();
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return Result.Failure(Error.NotFound("Device.NotFound",
                $"Sensor {sensorId} or Relay {relayId} does not exist."));
        }
        catch (RpcException ex)
        {
            return Result.Failure(Error.Failure("Grpc.Error",
                $"Communication failed: {ex.Status.Detail}"));
        }
    }
}
