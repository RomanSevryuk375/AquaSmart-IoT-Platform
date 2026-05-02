using Contracts.Events.TelemetryEvents;
using Contracts.Exceptions;
using Contracts.Results;
using MassTransit;
using Telemetry.Application.DTOs;
using Telemetry.Application.Interfaces;
using Telemetry.Domain.Entities;
using Telemetry.Domain.Interfaces;
using Telemetry.Domain.SpecificationParams;
using Telemetry.Domain.Specifications;

namespace Telemetry.Application.Services;

public class TelemetryDataService(
    ITelemetryRawDataRepository telemetryRepository,
    ISensorRepository sensorRepository,
    IEcosystemRepository ecosystemRepository,
    IPublishEndpoint publishEndpoint,
    IUnitOfWork unitOfWork) : ITelemetryDataService
{
    public async Task<Result<TelemetryRawChartResponseDto>> GetAllDataAsync(
        TelemetryDataFilterDto filter,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var to = filter.To ?? DateTime.UtcNow;
        var from = filter.From ?? to.AddDays(-1);

        var sensor = await sensorRepository
            .GetByIdAsync(filter.SensorId, cancellationToken);

        if (sensor is null)
        {
            return Result<TelemetryRawChartResponseDto>
                .Failure(Error.NotFound(
                    "Sensor.NotFound", 
                    $"Sensor {filter.SensorId} not found"));
        }

        var specification = new TelemetryFilterSpecification(
            new TelemetryFilterParams
            {
                SensorId = filter.SensorId,
                From = filter.From,
                To = filter.To,
            });

        var data = await telemetryRepository.GetAllAsync(
            specification,
            skip,
            take,
            cancellationToken);

        IEnumerable<TelemetryRawChartPointDto> points;

        points = data.Select(x => new TelemetryRawChartPointDto
        {
            Value = x.Value,
            RecordedAt = x.RecordedAt,
        });

        return Result<TelemetryRawChartResponseDto>.Success(
            new TelemetryRawChartResponseDto
            {
                SensorId = sensor.Id,
                SensorName = sensor.Name,
                Unit = sensor.Unit,
                Points = points.OrderBy(p => p.RecordedAt).ToList()
            });
    }

    public async Task<ConsumerResult> AddDataAsync(
        TelemetryBatchEvent batch,
        CancellationToken cancellationToken)
    {
        var ecosystem = await ecosystemRepository
            .GetByControllerIdAsync(batch.ControllerId, cancellationToken);

        if (ecosystem is null)
        {
            return ConsumerResult
                .RetryableError($"Ecosystem for controller {batch.ControllerId} not found.");
        }

        var sensors = await sensorRepository
            .GetAllByEcosystemId(ecosystem.Id, cancellationToken);

        if (!sensors.Any())
        {
            return ConsumerResult
                .FatalError($"Any sensors for ecosystem {ecosystem.Id} not found.");
        }

        var eventsToPublish = new List<TelemetryReceivedEvent>();

        foreach (var item in batch.Items)
        {
            var existingTelemetry = await telemetryRepository
                .GetByExternalMessageIdAsync(item.ExternalMessageId, cancellationToken);

            if (existingTelemetry is not null)
            {
                continue;
            }

            var sensor = sensors
                .FirstOrDefault(x => x.Id == item.SensorId);

            if (sensor is null)
            {
                continue;
            }

            var (telemetryData, errors) = TelemetryRawEntity.Create(
                item.SensorId,
                item.Value,
                item.ExternalMessageId,
                item.RecordedAt);

            if (telemetryData is null)
            {
                continue;
            }

            sensor.UpdateLastValue(item.Value);

            await telemetryRepository.AddAsync(telemetryData!, cancellationToken);
            await sensorRepository.UpdateAsync(sensor, cancellationToken);

            eventsToPublish.Add(new TelemetryReceivedEvent
            {
                SensorId = item.SensorId,
                Value = item.Value,
                RecordedAt = item.RecordedAt,
            });
        }

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return ConsumerResult.RetryableError($"Database error: {ex.Message}");
        }

        foreach (var item in eventsToPublish)
        {
            await publishEndpoint.Publish(item, cancellationToken);
        }

        return ConsumerResult.Success();
    }
}
