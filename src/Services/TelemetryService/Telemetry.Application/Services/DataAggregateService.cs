using Contracts.Enums;
using Contracts.Results;
using MassTransit.Initializers;
using Telemetry.Application.DTOs;
using Telemetry.Application.Interfaces;
using Telemetry.Domain.Interfaces;
using Telemetry.Domain.SpecificationParams;
using Telemetry.Domain.Specifications;

namespace Telemetry.Application.Services;

public sealed class DataAggregateService(
    ITelemetryAggregateDataRepository dataRepository,
    ISensorRepository sensorRepository) : IDataAggregateService
{
    public async Task<Result<TelemetryChartResponseDto>> GetChartDataAsync(
        TelemetryAggregateFilterDto filter,
        int? skip,
        int? take,
        CancellationToken cancellationToken)
    {
        var to = filter.To ?? DateTime.UtcNow;
        var from = filter.From ?? to.AddDays(-1);

        var sensor = await sensorRepository
            .GetByIdAsync(filter.SensorId, cancellationToken);

        if (sensor is null)
        {
            return Result<TelemetryChartResponseDto>
                .Failure(Error.NotFound("Sensor.NotFound", "Sensor not found"));
        }

        var period = filter.Period
            ?? DetermineBestPeriod(from, to);

        IEnumerable<TelemetryChartPointDto> points;

        var specification = new TelemetryAggregateFilterSpecification(
            new TelemetryAggregateFilterParams
            {
                SensorId = filter.SensorId,
                Period = period,
                From = from,
                To = to,
            });

        var data = await dataRepository.GetAllAsync(
            specification,
            skip,
            take,
            cancellationToken);

        points = data.Select(x => new TelemetryChartPointDto
        {
            AvgValue = x.AvgValue,
            MinValue = x.MinValue,
            MaxValue = x.MaxValue,
            Time = x.PeriodStart,
        });


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

        if (duration.TotalHours <= 6)
        {
            return PeriodTypeEnum.Minute;
        }

        if (duration.TotalDays <= 7)
        {
            return PeriodTypeEnum.Hourly;
        }

        return PeriodTypeEnum.Daily;
    }
}
