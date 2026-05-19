using AutoMapper;
using Contracts.Enums;
using Contracts.Results;
using Telemetry.Application.DTOs;
using Telemetry.Application.Interfaces;
using Telemetry.Domain.Interfaces;
using Telemetry.Domain.SpecificationParams;
using Telemetry.Domain.Specifications;

namespace Telemetry.Application.Services;

public sealed class DataAggregateService(
    ITelemetryAggregateDataRepository dataRepository,
    ISensorRepository sensorRepository,
    IMapper mapper) : IDataAggregateService
{
    private const int MaxHoursForMinuteInterval = 6;
    private const int MaxDaysForHourlyInterval = 7;

    public async Task<Result<TelemetryChartResponseDto>> GetChartDataAsync(
        TelemetryAggregateFilterDto filter,
        int? skip,
        int? take,
        CancellationToken cancellationToken)
    {
        var to = filter.To ?? DateTime.UtcNow;
        var from = filter.From ?? to.AddDays(-1);

        var sensor = await sensorRepository.GetByIdAsync(filter.SensorId, cancellationToken);
        if (sensor is null)
        {
            return Result<TelemetryChartResponseDto>
                .Failure(Error.NotFound("Sensor.NotFound", "Sensor not found"));
        }

        var period = filter.Period ?? DetermineBestPeriod(from, to);

        var specification = new TelemetryAggregateFilterSpecification(
            new TelemetryAggregateFilterParams
            {
                SensorId = filter.SensorId,
                Period = period,
                From = from,
                To = to,
            });

        var data = await dataRepository.GetAllAsync(specification, skip, take, cancellationToken);

        var points = mapper.Map<IReadOnlyList<TelemetryChartPointDto>>(data);

        return Result<TelemetryChartResponseDto>
            .Success(new TelemetryChartResponseDto
            {
                SensorId = sensor.Id,
                SensorName = sensor.Name,
                Unit = sensor.Unit,
                Points = points.OrderBy(p => p.Time).ToList()
            });
    }

    private static PeriodTypeEnum DetermineBestPeriod(DateTime from, DateTime to)
    {
        var duration = to - from;

        if (duration.TotalHours <= MaxHoursForMinuteInterval)
        {
            return PeriodTypeEnum.Minute;
        }

        if (duration.TotalDays <= MaxDaysForHourlyInterval)
        {
            return PeriodTypeEnum.Hourly;
        }

        return PeriodTypeEnum.Daily;
    }
}
