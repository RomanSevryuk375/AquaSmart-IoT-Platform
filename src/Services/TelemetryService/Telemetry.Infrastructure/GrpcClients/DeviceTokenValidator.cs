// Ignore Spelling: Validator

using Contracts.Constants;
using Contracts.gRPC.Devices;
using Contracts.Results;
using Grpc.Core;
using Telemetry.Application.DTOs;
using Telemetry.Application.Interfaces;

namespace Telemetry.Infrastructure.GrpcClients;

public sealed class DeviceTokenValidator(DeviceIntegrationGrpc.DeviceIntegrationGrpcClient client)
    : IDeviceTokenValidator
{
    public async Task<Result<ValidateResponseDto>> ValidateAsync(
        string macAddress,
        string deviceToken,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new ValidateTokenRequest
            {
                MacAddress = macAddress,
                DeviceToken = deviceToken
            };

            ValidateTokenResponse response = await client.ValidateTokenAsync(
                request, cancellationToken: cancellationToken);
            if (!response.IsValid)
            {
                return Result<ValidateResponseDto>.Failure(Error.Conflict(
                    ErrorCodes.Security.AccessDenied,
                    ErrorMessages.Security.ControllerInvalid));
            }

            if (!Guid.TryParse(response.ControllerId, out Guid controllerId) ||
                !Guid.TryParse(response.UserId, out Guid userId))
            {
                return Result<ValidateResponseDto>.Failure(Error.Validation(
                    ErrorCodes.Validation.GuidInvalid,
                    string.Format(ErrorMessages.Validation.GuidInvalidFormat, response.UserId, response.ControllerId)));
            }
            return Result<ValidateResponseDto>.Success(new ValidateResponseDto
            {
                ControllerId = controllerId,
                UserId = userId
            });
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return Result<ValidateResponseDto>.Failure(Error.Conflict(
                ErrorCodes.Grpc.Conflict,
                ex.Message));
        }
        catch (RpcException ex)
        {
            return Result<ValidateResponseDto>.Failure(Error.Failure(
                ErrorCodes.Grpc.Error,
                string.Format(ErrorMessages.Grpc.CommunicationFailedFormat, ex.Status.Detail)));
        }
    }
}
