using Contracts.Enums;
using Contracts.Results;
using MassTransit;
using Telemetry.Application.Interfaces;
using Telemetry.Domain.Entities;
using Telemetry.Domain.Interfaces;
using Telemetry.Domain.ValueObjects;

namespace Telemetry.Application.Features.BackgroundJobs.Commands.Shared;

public sealed class CompressorHelper(
    ITelemetryAggregateDataRepository telemetryAggregate,
    ISensorRepository sensorRepository) : ICompressorHelper
{
    public async Task CreateAndSaveAggregatedTelemetryAsync(
        Guid sensorId,
        TelemetrySummary summary,
        DateTime from,
        PeriodType periodType,
        CancellationToken cancellationToken = default)
    {
        Sensor? sensor = await sensorRepository.GetByIdAsync(sensorId, cancellationToken);
        if (sensor is null)
        {
            return;
        }

        Result<AggregateTelemetry> result = AggregateTelemetry.Create(
            id: NewId.NextGuid(), sensor.Id, sensor.EcosystemId,
            from, periodType,
            summary.MinValue, summary.MaxValue, summary.AvgValue,
            summary.Count);
        if (result.IsFailure)
        {
            return;
        }

        await telemetryAggregate.AddAsync(result.Value, cancellationToken);
    }
}
