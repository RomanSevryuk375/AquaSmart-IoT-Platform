using Contracts.gRPC.Devices;
using Device.Application.Features.Integrations.GetDeviceMetadata;
using Device.Application.Features.Integrations.ValidateDeviceToken;
using Device.Application.Features.Integrations.ValidateHardwareAssignment;
using Grpc.Core;
using MediatR;

namespace Device.API.gRPC;

public sealed class DeviceIntegrationEndpoint(ISender sender)
    : DeviceIntegrationGrpc.DeviceIntegrationGrpcBase
{
    public override async Task<GetMetadataResponse> GetMetadata(
        GetMetadataRequest request,
        ServerCallContext context)
    {
        var query = new GetDeviceMetadataQuery
        {
            ControllerId = Guid.TryParse(request.ControllerId, out Guid cid) ? cid : null,
            SensorId = Guid.TryParse(request.SensorId, out Guid sid) ? sid : null,
            RelayId = Guid.TryParse(request.RelayId, out Guid rid) ? rid : null
        };

        Result<DeviceMetadataDto> result = await sender.Send(query, context.CancellationToken);

        if (result.IsFailure)
        {
            throw new RpcException(new Status(StatusCode.NotFound, result.Error.Message));
        }

        return new GetMetadataResponse
        {
            ControllerName = result.Value.ControllerName ?? string.Empty,
            SensorName = result.Value.SensorName ?? string.Empty,
            RelayName = result.Value.RelayName ?? string.Empty
        };
    }

    public override async Task<ValidateTokenResponse> ValidateToken(
        ValidateTokenRequest request,
        ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.DeviceToken) ||
            string.IsNullOrWhiteSpace(request.MacAddress))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid device token or MAC address"));
        }

        var query = new ValidateDeviceTokenQuery
        {
            MacAddress = request.MacAddress,
            RawDeviceToken = request.DeviceToken,
        };

        Result<ValidateDeviceTokenDto> result = await sender.Send(query, context.CancellationToken);
        if (result.IsFailure)
        {
            throw new RpcException(new Status(StatusCode.NotFound, result.Error.Message));
        }

        return new ValidateTokenResponse
        {
            IsValid = result.Value.IsValid,
            ControllerId = result.Value.ControllerId.ToString(),
            UserId = result.Value.UserId.ToString(),
        };
    }

    public override async Task<ValidateHardwareAssignmentResponse> ValidateHardwareAssignment(
        ValidateHardwareAssignmentRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.SensorId, out Guid sensorId) ||
            !Guid.TryParse(request.RelayId, out Guid relayId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Guid format"));
        }

        var query = new ValidateHardwareAssignmentQuery
        {
            RelayId = relayId,
            SensorId = sensorId
        };

        Result<ValidateHardwareAssignmentDto> result = await sender.Send(query, context.CancellationToken);
        if (result.IsFailure)
        {
            throw new RpcException(new Status(StatusCode.NotFound, result.Error.Message));
        }

        return new ValidateHardwareAssignmentResponse { IsValid = result.Value.IsValid };
    }
}
