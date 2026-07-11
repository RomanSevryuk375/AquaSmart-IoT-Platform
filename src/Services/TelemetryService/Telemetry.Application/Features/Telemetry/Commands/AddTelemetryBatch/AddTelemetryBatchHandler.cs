using Contracts.Constants;
using Contracts.Events.TelemetryEvents;
using Contracts.Results;
using MassTransit;
using MediatR;
using Telemetry.Application.DTOs;
using Telemetry.Application.Interfaces;
using Telemetry.Domain.Entities;
using Telemetry.Domain.Interfaces;

namespace Telemetry.Application.Features.Telemetry.Commands.AddTelemetryBatch;

internal sealed class AddTelemetryBatchHandler(
    ITelemetryRawDataRepository telemetryRepository,
    ISensorRepository sensorRepository,
    IEcosystemRepository ecosystemRepository,
    IDeviceTokenValidator deviceTokenValidator) : IRequestHandler<AddTelemetryBatchCommand, Result>
{
    public async Task<Result> Handle(AddTelemetryBatchCommand request, CancellationToken cancellationToken)
    {
        Result<ValidateResponseDto> validationResult = await deviceTokenValidator.ValidateAsync(
            request.MacAddress, request.DeviceToken, cancellationToken);
        if (validationResult.IsFailure)
        {
            return Result.Failure(validationResult.Error);
        }

        Ecosystem? ecosystem = await ecosystemRepository.GetByControllerIdAsync(
            validationResult.Value.ControllerId, cancellationToken);
        if (ecosystem is null)
        {
            return Result.Failure(Error.NotFound<Ecosystem>(
                $"Ecosystem for controller {validationResult.Value.ControllerId} not found."));
        }

        if (ecosystem.UserId != validationResult.Value.UserId)
        {
            return Result.Failure(Error.Conflict(ErrorMessages.AccessDenied,
                ErrorMessages.YouDontOwnThisController));
        }


        IReadOnlyList<Sensor> sensors = await sensorRepository.GetAllByEcosystemId(
            ecosystem.Id, cancellationToken);
        if (sensors.Count == 0)
        {
            return Result.Failure(Error.NotFound<Sensor>(
                $"No sensors found for ecosystem {ecosystem.Id}."));
        }

        foreach (TelemetryBatchEventItem item in request.Items)
        {
            RawTelemetry? existingTelemetry = await telemetryRepository.GetByExternalMessageIdAsync(
                item.ExternalMessageId, cancellationToken);

            if (existingTelemetry is not null)
            {
                continue;
            }

            Sensor? sensor = sensors.FirstOrDefault(x => x.Id == item.SensorId);
            if (sensor is null)
            {
                continue;
            }

            Result<RawTelemetry> telemetryResult = RawTelemetry.Create(
                id: NewId.NextGuid(), item.SensorId, ecosystem.Id,
                item.Value, item.ExternalMessageId, item.RecordedAt);
            if (telemetryResult.IsFailure)
            {
                continue;
            }

            sensor.UpdateLastValue(item.Value);
            await telemetryRepository.AddAsync(telemetryResult.Value, cancellationToken);
        }

        return Result.Success();
    }
}
