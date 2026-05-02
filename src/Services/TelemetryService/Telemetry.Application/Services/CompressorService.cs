using Contracts.Enums;
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
    public async Task CompressToMinutesAsync(
        CancellationToken cancellationToken)
    {
        var to = new DateTime(
            DateTime.UtcNow.Year,
            DateTime.UtcNow.Month,
            DateTime.UtcNow.Day,
            DateTime.UtcNow.Hour,
            DateTime.UtcNow.Minute,
            0,
            DateTimeKind.Utc).AddMinutes(-1);

        var from = to.AddMinutes(-1);
        var data = await telemetryRaw
            .GetSummaryForPeriodAsync(from, to, cancellationToken);

        if (data is null)
        {
            return;
        }

        foreach (var item in data)
        {
            var (aggregate, _) = TelemetryAggregateEntity.Create(
                item.SensorId,
                from,
                PeriodTypeEnum.Minute,
                item.MinValue,
                item.MaxValue,
                item.AvgValue,
                item.Count);

            if (aggregate is null)
            {
                continue;
            }

            await telemetryAggregate.AddAsync(aggregate, cancellationToken);
        }

        var sensorIds = data.Select(x => x.SensorId).ToList();

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await telemetryRaw.MarkAsAggregatedAsync(sensorIds, from, to, cancellationToken);
    }

    public async Task CompressToHoursAsync(
        CancellationToken cancellationToken)
    {
        var to = new DateTime(
            DateTime.UtcNow.Year,
            DateTime.UtcNow.Month,
            DateTime.UtcNow.Day,
            DateTime.UtcNow.Hour,
            0,
            0,
            DateTimeKind.Utc);

        var from = to.AddHours(-1);
        var data = await telemetryAggregate
            .GetSummaryForPeriodAsync(PeriodTypeEnum.Minute, from, to, cancellationToken);

        if (data is null)
        {
            return;
        }

        var sensorIds = new List<Guid>();

        foreach (var item in data)
        {
            var (aggregate, _) = TelemetryAggregateEntity.Create(
                item.SensorId,
                from,
                PeriodTypeEnum.Hourly,
                item.MinValue,
                item.MaxValue,
                item.AvgValue,
                item.Count);

            if (aggregate is null)
            {
                continue;
            }

            await telemetryAggregate.AddAsync(aggregate, cancellationToken);

            sensorIds.Add(item.SensorId);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await telemetryAggregate.MarkAsAggregatedAsync(sensorIds, from, to, cancellationToken);
    }

    public async Task CompressToDaysAsync(
        CancellationToken cancellationToken)
    {
        var to = new DateTime(
            DateTime.UtcNow.Year,
            DateTime.UtcNow.Month,
            DateTime.UtcNow.Day,
            0,
            0,
            0,
            DateTimeKind.Utc);

        var from = to.AddHours(-24);
        var data = await telemetryAggregate
            .GetSummaryForPeriodAsync(PeriodTypeEnum.Hourly, from, to, cancellationToken);

        if (data is null)
        {
            return;
        }

        var sensorIds = new List<Guid>();

        foreach (var item in data)
        {
            var (aggregate, _) = TelemetryAggregateEntity.Create(
                item.SensorId,
                from,
                PeriodTypeEnum.Daily,
                item.MinValue,
                item.MaxValue,
                item.AvgValue,
                item.Count);

            if (aggregate is null)
            {
                continue;
            }

            await telemetryAggregate.AddAsync(aggregate, cancellationToken);

            sensorIds.Add(item.SensorId);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await telemetryAggregate.MarkAsAggregatedAsync(sensorIds, from, to, cancellationToken);
    }
}
