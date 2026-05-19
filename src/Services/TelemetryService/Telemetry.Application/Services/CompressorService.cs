using Contracts.Enums;
using Contracts.Results;
using MassTransit.Initializers;
using Telemetry.Application.Interfaces;
using Telemetry.Domain.Entities;
using Telemetry.Domain.Interfaces;

namespace Telemetry.Application.Services;

public sealed class CompressorService(
    ITelemetryAggregateDataRepository telemetryAggregate,
    ITelemetryRawDataRepository telemetryRaw,
    IUnitOfWork unitOfWork) : ICompressorService
{
    private const int minuteInterval = -1;
    private const int hourlyInterval = -1;
    private const int dailyInterval = -24;

    public async Task<Result> CompressToMinutesAsync(CancellationToken cancellationToken)
    {
        var to = new DateTime(
            DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day,
            DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, 0, DateTimeKind.Utc);
        var from = to.AddMinutes(minuteInterval);

        var data = await telemetryRaw.GetSummaryForPeriodAsync(from, to, cancellationToken);
        if (data is null)
        {
            return Result.Success();
        }

        foreach (var item in data)
        {
            await CreateAndSaveAggregatedTelemetryAsync(
                item, from, PeriodTypeEnum.Minute, cancellationToken);
        }

        var sensorIds = data.Select(x => x.SensorId).ToList();

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await telemetryRaw.MarkAsAggregatedAsync(sensorIds, from, to, cancellationToken);

        return Result.Success();
    }

    public async Task<Result> CompressToHoursAsync(CancellationToken cancellationToken)
    {
        var to = new DateTime(
            DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day,
            DateTime.UtcNow.Hour, 0, 0, DateTimeKind.Utc);
        var from = to.AddHours(hourlyInterval);

        var data = await telemetryAggregate.GetSummaryForPeriodAsync(
            PeriodTypeEnum.Minute, from, to, cancellationToken);
        if (data is null)
        {
            return Result.Success();
        }

        var sensorIds = new List<Guid>();

        foreach (var item in data)
        {
            await CreateAndSaveAggregatedTelemetryAsync(item, from, PeriodTypeEnum.Hourly, cancellationToken);
            sensorIds.Add(item.SensorId);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await telemetryAggregate.MarkAsAggregatedAsync(sensorIds, from, to, cancellationToken);

        return Result.Success();
    }

    public async Task<Result> CompressToDaysAsync(CancellationToken cancellationToken)
    {
        var to = new DateTime(
            DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day,
            0, 0, 0, DateTimeKind.Utc);
        var from = to.AddHours(dailyInterval);

        var data = await telemetryAggregate.GetSummaryForPeriodAsync(
            PeriodTypeEnum.Hourly, from, to, cancellationToken);
        if (data is null)
        {
            return Result.Success();
        }

        var sensorIds = new List<Guid>();

        foreach (var item in data)
        {
            await CreateAndSaveAggregatedTelemetryAsync(item, from, PeriodTypeEnum.Daily, cancellationToken);
            sensorIds.Add(item.SensorId);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await telemetryAggregate.MarkAsAggregatedAsync(sensorIds, from, to, cancellationToken);

        return Result.Success();
    }

    private async Task CreateAndSaveAggregatedTelemetryAsync(
        TelemetrySummary telemetry,
        DateTime from,
        PeriodTypeEnum periodType,
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
}
