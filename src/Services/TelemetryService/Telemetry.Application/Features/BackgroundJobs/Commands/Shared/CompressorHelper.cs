using Contracts.Enums;
using Contracts.Results;
using MassTransit;
using Telemetry.Application.DTOs;
using Telemetry.Application.Interfaces;
using Telemetry.Domain.Entities;
using Telemetry.Domain.Interfaces;
using Telemetry.Domain.ValueObjects;

namespace Telemetry.Application.Features.BackgroundJobs.Commands.Shared;

public sealed class CompressorHelper(
    ITelemetryAggregateDataRepository telemetryAggregate,
    ISensorRepository sensorRepository,
    ITelemetryNotifier telemetryNotifier) : ICompressorHelper
{
    public async Task CreateAndSaveAggregatedTelemetryAsync(
        Guid sensorId,
        TelemetrySummary summary,
        DateTime from,
        PeriodType periodType,
        CancellationToken cancellationToken = default)
    {
        Result<AggregateTelemetry> result = AggregateTelemetry.Create(
            id: NewId.NextGuid(),
            sensorId,
            from,
            periodType,
            summary.MinValue,
            summary.MaxValue,
            summary.AvgValue,
            summary.Count);

        if (result.IsFailure)
        {
            return;
        }

        await telemetryAggregate.AddAsync(result.Value, cancellationToken);
    }

    public async Task NotifyClientsAsync(
        IReadOnlyDictionary<Guid, TelemetrySummary> data,
        DateTime periodStart,
        PeriodType periodType,
        CancellationToken cancellationToken = default)
    {
        var sensorIds = data.Keys.ToList();
        IReadOnlyList<Sensor> sensors = await sensorRepository.GetManyByIdsAsync(sensorIds, cancellationToken);

        var ecosystemMap = sensors.ToDictionary(
            s => s.Id,
            s => s.EcosystemId.ToString());

        foreach (KeyValuePair<Guid, TelemetrySummary> kvp in data)
        {
            Guid sensorId = kvp.Key;
            TelemetrySummary summary = kvp.Value;

            if (!ecosystemMap.TryGetValue(sensorId, out string? ecosystemId))
            {
                continue;
            }

            var point = new TelemetryChartPointDto
            {
                SensorId = sensorId,
                MinValue = summary.MinValue,
                MaxValue = summary.MaxValue,
                AvgValue = summary.AvgValue,
                Time = periodStart
            };

            await telemetryNotifier.AggregatePointGenerated(ecosystemId, periodType, point);
        }
    }
}
