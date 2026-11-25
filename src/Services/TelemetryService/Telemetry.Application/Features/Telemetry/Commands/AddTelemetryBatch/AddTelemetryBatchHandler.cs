using BuildingBlocks.Domain.Results;
using MediatR;
using Telemetry.Application.DTOs;
using Telemetry.Application.Interfaces;

namespace Telemetry.Application.Features.Telemetry.Commands.AddTelemetryBatch;

internal sealed class AddTelemetryBatchHandler(
    IDeviceTokenValidator deviceTokenValidator,
    ITelemetryBatchIngestionService ingestionService) : IRequestHandler<AddTelemetryBatchCommand, Result>
{
    public async Task<Result> Handle(AddTelemetryBatchCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.DeviceToken))
        {
            return Result.Failure(Error.Unauthorized("Device.MissingToken", "Device token is required."));
        }

        Result<ValidateResponseDto> validationResult = await deviceTokenValidator.ValidateAsync(
            request.MacAddress, request.DeviceToken, cancellationToken);
        if (validationResult.IsFailure)
        {
            return Result.Failure(validationResult.Error);
        }

        return await ingestionService.IngestAsync(
            validationResult.Value.ControllerId,
            validationResult.Value.UserId,
            request.Items,
            cancellationToken);
    }
}
