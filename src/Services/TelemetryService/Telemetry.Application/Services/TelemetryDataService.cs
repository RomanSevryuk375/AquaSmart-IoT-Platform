using AutoMapper;
using Contracts.Events.TelemetryEvents;
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
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ITelemetryNotifier realtimeNotifier) : ITelemetryDataService
{
    private const int DefaultPeriodDays = -1;

    public async Task<Result<TelemetryRawChartResponseDto>> GetAllDataAsync(
        TelemetryDataFilterDto filter,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var to = filter.To ?? DateTime.UtcNow;
        var from = filter.From ?? to.AddDays(DefaultPeriodDays);

        var sensor = await sensorRepository.GetByIdAsync(filter.SensorId, cancellationToken);
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
                From = from,
                To = to,
            });

        var data = await telemetryRepository.GetAllAsync(specification, skip, take, cancellationToken);

        var points = mapper.Map<IReadOnlyList<TelemetryRawChartPointDto>>(data);

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
        var ecosystem = await ecosystemRepository.GetByControllerIdAsync(batch.ControllerId, cancellationToken);
        if (ecosystem is null)
        {
            return ConsumerResult.RetryableError(
                $"Ecosystem for controller {batch.ControllerId} not found.");
        }

        var sensors = await sensorRepository.GetAllByEcosystemId(ecosystem.Id, cancellationToken);
        if (!sensors.Any())
        {
            return ConsumerResult.FatalError(
                $"No sensors found for ecosystem {ecosystem.Id}.");
        }

        var points = new List<TelemetryRawChartPointDto>();

        foreach (var item in batch.Items)
        {
            var existingTelemetry = await telemetryRepository.GetByExternalMessageIdAsync(
                item.ExternalMessageId, cancellationToken);
            if (existingTelemetry is not null)
            {
                continue;
            }

            var sensor = sensors.FirstOrDefault(x => x.Id == item.SensorId);
            if (sensor is null)
            {
                continue;
            }

            var result = TelemetryRawEntity.Create(
                item.SensorId, 
                item.Value, 
                item.ExternalMessageId,
                item.RecordedAt);
            if (result.IsFailure)
            {
                continue;
            }

            sensor.UpdateLastValue(item.Value);

            await telemetryRepository.AddAsync(result.Value, cancellationToken);
            await sensorRepository.UpdateAsync(sensor, cancellationToken);

            await publishEndpoint.Publish(
                mapper.Map<TelemetryReceivedEvent>(item), cancellationToken);

            points.Add(mapper.Map<TelemetryRawChartPointDto>(item));
        }

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return ConsumerResult.RetryableError($"Database error: {ex.Message}");
        }

        foreach (var point in points)
        {
            await realtimeNotifier.TelemetryRawReceived(ecosystem.Id.ToString(), point);
        }

        return ConsumerResult.Success();
    }
}