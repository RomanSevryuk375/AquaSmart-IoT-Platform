using Contracts.Enums;
using Contracts.Results;
using MassTransit.Initializers;
using System.Data;
using Telemetry.Application.DTOs;
using Telemetry.Application.Interfaces;
using Telemetry.Domain.Entities;
using Telemetry.Domain.Interfaces;

namespace Telemetry.Application.Services;

public sealed class CompressorService(
    ITelemetryAggregateDataRepository telemetryAggregate,
    ITelemetryRawDataRepository telemetryRaw,
    ISensorRepository sensorRepository,
    IUnitOfWork unitOfWork,
    ITelemetryNotifier telemetryNotifier) : ICompressorService
{
    private const int MinuteInterval = -1;
    private const int HourlyInterval = -1;
    private const int DailyInterval = -24;

    public async Task<Result> CompressToMinutesAsync(CancellationToken cancellationToken)
    {
        var to = new DateTime(
            DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day,
            DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, 0, DateTimeKind.Utc);
        var from = to.AddMinutes(MinuteInterval);

        var data = await telemetryRaw.GetSummaryForPeriodAsync(from, to, cancellationToken);
        if (data is null)
        {
            return Result.Success();
        }

        foreach (var item in data)
        {
            await CreateAndSaveAggregatedTelemetryAsync(
                item, from, PeriodType.Minute, cancellationToken);
        }

        var sensorIds = data.Select(x => x.SensorId).ToList();

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await telemetryRaw.MarkAsAggregatedAsync(sensorIds, from, to, cancellationToken);

        await NotifyClientsAsync(data, from, PeriodType.Minute, cancellationToken);

        return Result.Success();
    }

    public async Task<Result> CompressToHoursAsync(CancellationToken cancellationToken)
    {
        var to = new DateTime(
            DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day,
            DateTime.UtcNow.Hour, 0, 0, DateTimeKind.Utc);
        var from = to.AddHours(HourlyInterval);

        var data = await telemetryAggregate.GetSummaryForPeriodAsync(
            PeriodType.Minute, from, to, cancellationToken);
        if (data is null)
        {
            return Result.Success();
        }

        var sensorIds = new List<Guid>();

        foreach (var item in data)
        {
            await CreateAndSaveAggregatedTelemetryAsync(
                item, from, PeriodType.Hourly, cancellationToken);

            sensorIds.Add(item.SensorId);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await telemetryAggregate.MarkAsAggregatedAsync(sensorIds, from, to, cancellationToken);

        await NotifyClientsAsync(data, from, PeriodType.Hourly, cancellationToken);

        return Result.Success();
    }

    public async Task<Result> CompressToDaysAsync(CancellationToken cancellationToken)
    {
        var to = new DateTime(
            DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day,
            0, 0, 0, DateTimeKind.Utc);
        var from = to.AddHours(DailyInterval);

        var data = await telemetryAggregate.GetSummaryForPeriodAsync(
            PeriodType.Hourly, from, to, cancellationToken);
        if (data is null)
        {
            return Result.Success();
        }

        var sensorIds = new List<Guid>();
        foreach (var item in data)
        {
            await CreateAndSaveAggregatedTelemetryAsync(
                item, from, PeriodType.Daily, cancellationToken);

            sensorIds.Add(item.SensorId);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await telemetryAggregate.MarkAsAggregatedAsync(sensorIds, from, to, cancellationToken);

        await NotifyClientsAsync(data, from, PeriodType.Daily, cancellationToken);

        return Result.Success();
    }

    private async Task CreateAndSaveAggregatedTelemetryAsync(
        TelemetrySummary telemetry,
        DateTime from,
        PeriodType periodType,
        CancellationToken cancellationToken)
    {
        var result = TelemetryAggregateEntity.Create(
            telemetry.SensorId,
            from,
            periodType,
            telemetry.MinValue,
            telemetry.MaxValue,
            telemetry.AvgValue,
            telemetry.Count);
        if (result.IsFailure)
        {
            return;
        }

        await telemetryAggregate.AddAsync(result.Value, cancellationToken);
    }

    private async Task NotifyClientsAsync(
        IReadOnlyList<TelemetrySummary> data,
        DateTime periodStart,
        PeriodType periodType,
        CancellationToken cancellationToken)
    {
        var sensorIds = data.Select(x => x.SensorId).Distinct().ToList();
        var sensors = await sensorRepository.GetManyByIdsAsync(sensorIds, cancellationToken);
        var ecosystemMap = sensors.ToDictionary(s => s.Id, s => s.EcosystemId.ToString());

        foreach (var item in data)
        {
            if (!ecosystemMap.TryGetValue(item.SensorId, out var ecosystemId))
            {
                continue; 
            }

            var point = new TelemetryChartPointDto
            {
                SensorId = item.SensorId,
                MinValue = item.MinValue,
                MaxValue = item.MaxValue,
                AvgValue = item.AvgValue,
                Time = periodStart 
            };

            await telemetryNotifier.AggregatePointGenerated(ecosystemId, periodType, point);
        }
    }
}
