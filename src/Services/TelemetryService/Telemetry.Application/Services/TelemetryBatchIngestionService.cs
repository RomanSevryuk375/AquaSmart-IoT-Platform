using BuildingBlocks.Domain.Constants;
using BuildingBlocks.Domain.Results;
using BuildingBlocks.IntegrationEvents.Events.Telemetrys;
using MassTransit;
using Telemetry.Application.Interfaces;
using Telemetry.Domain.Entities;
using Telemetry.Domain.Interfaces;

namespace Telemetry.Application.Services;

public sealed class TelemetryBatchIngestionService(
    ITelemetryRawDataRepository telemetryRepository,
    ISensorRepository sensorRepository,
    IEcosystemRepository ecosystemRepository) : ITelemetryBatchIngestionService
{
    public async Task<Result> IngestAsync(
        Guid controllerId,
        Guid userId,
        IReadOnlyList<TelemetryBatchEventItem> items,
        CancellationToken cancellationToken)
    {
        Ecosystem? ecosystem = await ecosystemRepository.GetByControllerIdAsync(
            controllerId, cancellationToken);
        if (ecosystem is null)
        {
            return Result.Failure(Error.NotFound<Ecosystem>(
                $"Ecosystem for controller {controllerId} not found."));
        }

        if (ecosystem.UserId != userId)
        {
            return Result.Failure(Error.Forbidden(
                ErrorMessages.AccessDenied,
                ErrorMessages.YouDontOwnThisController));
        }

        IReadOnlyList<Sensor> sensors = await sensorRepository.GetAllByEcosystemId(
            ecosystem.Id, cancellationToken);
        if (sensors.Count == 0)
        {
            return Result.Failure(Error.NotFound<Sensor>(
                $"No sensors found for ecosystem {ecosystem.Id}."));
        }

        var sensorsById = sensors.ToDictionary(s => s.Id);
        var uniqueItems = items
            .Where(x => !string.IsNullOrWhiteSpace(x.ExternalMessageId))
            .DistinctBy(x => x.ExternalMessageId)
            .ToList();

        int addedCount = 0;
        foreach (TelemetryBatchEventItem item in uniqueItems)
        {
            if (!sensorsById.TryGetValue(item.SensorId, out Sensor? sensor))
            {
                continue;
            }

            RawTelemetry? existingTelemetry = await telemetryRepository.GetByExternalMessageIdAsync(
                item.ExternalMessageId, cancellationToken);
            if (existingTelemetry is not null)
            {
                continue;
            }

            Result<RawTelemetry> telemetryResult = RawTelemetry.Create(
                id: NewId.NextGuid(),
                item.SensorId,
                ecosystem.Id,
                item.Value,
                item.ExternalMessageId,
                item.RecordedAt);
            if (telemetryResult.IsFailure)
            {
                continue;
            }

            sensor.UpdateLastValue(item.Value);
            await telemetryRepository.AddAsync(telemetryResult.Value, cancellationToken);
            addedCount++;
        }

        if (addedCount > 0)
        {
            await telemetryRepository.SaveBatchWithDuplicateHandlingAsync(cancellationToken);
        }

        return Result.Success();
    }
}
