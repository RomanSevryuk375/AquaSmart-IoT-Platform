using AutoMapper;
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
    IPublishEndpoint publishEndpoint,
    IMapper mapper,
    ITelemetryNotifier realtimeNotifier) : IRequestHandler<AddTelemetryBatchCommand, Result>
{
    public async Task<Result> Handle(AddTelemetryBatchCommand request, CancellationToken cancellationToken)
    {
        Ecosystem? ecosystem = await ecosystemRepository.GetByControllerIdAsync(
            request.ControllerId, cancellationToken);
        if (ecosystem is null)
        {
            return Result.Failure(Error.NotFound<Ecosystem>(
                $"Ecosystem for controller {request.ControllerId} not found."));
        }

        IReadOnlyList<Sensor> sensors = await sensorRepository.GetAllByEcosystemId(
            ecosystem.Id, cancellationToken);
        if (sensors.Count == 0)
        {
            return Result.Failure(Error.NotFound<Sensor>(
                $"No sensors found for ecosystem {ecosystem.Id}."));
        }

        var points = new List<TelemetryRawChartPointDto>();

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
                Guid.NewGuid(),
                item.SensorId,
                item.Value,
                item.ExternalMessageId,
                item.RecordedAt);

            if (telemetryResult.IsFailure)
            {
                continue;
            }

            sensor.UpdateLastValue(item.Value);

            await telemetryRepository.AddAsync(telemetryResult.Value, cancellationToken);

            await publishEndpoint.Publish(mapper.Map<TelemetryReceivedEvent>(item), cancellationToken);

            points.Add(mapper.Map<TelemetryRawChartPointDto>(item));
        }

        foreach (TelemetryRawChartPointDto point in points)
        {
            await realtimeNotifier.TelemetryRawReceived(ecosystem.Id.ToString(), point);
        }

        return Result.Success();
    }
}
