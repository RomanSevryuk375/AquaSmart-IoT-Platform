using Contracts.Abstractions;
using Contracts.Enums;
using Contracts.Results;
using Telemetry.Domain.Events;
using Telemetry.Domain.ValueObjects;

namespace Telemetry.Domain.Entities;

public sealed class AggregateTelemetry : AggregateRoot, IEntity
{
    private AggregateTelemetry(
        Guid id,
        Guid sensorId,
        DateTime periodStart,
        PeriodType period,
        TelemetrySummary summary,
        DateTime createdAt,
        bool isAggregated)
    {
        Id = id;
        SensorId = sensorId;
        PeriodStart = periodStart;
        Period = period;
        Summary = summary;
        CreatedAt = createdAt;
        IsAggregated = isAggregated;
    }

#pragma warning disable CS8618 
    private AggregateTelemetry() { }
#pragma warning restore CS8618 

    public Guid Id { get; private set; }
    public Guid SensorId { get; private set; }
    public DateTime PeriodStart { get; private set; }
    public PeriodType Period { get; private set; }
    public TelemetrySummary Summary { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsAggregated { get; private set; }


    public static Result<AggregateTelemetry> Create(
        Guid id, Guid sensorId, Guid ecosystemId,
        DateTime periodStart, PeriodType period,
        double minValue, double maxValue, double avgValue, int dataPointsCount)
    {
        Result<TelemetrySummary> summaryResult = TelemetrySummary.Create(
            minValue, avgValue, maxValue, dataPointsCount);
        if (summaryResult.IsFailure)
        {
            return Result<AggregateTelemetry>.Failure(summaryResult.Error);
        }

        TelemetrySummary summary = summaryResult.Value;

        var aggregateData = new AggregateTelemetry(
            id,
            sensorId,
            periodStart,
            period,
            summary,
            createdAt: DateTime.UtcNow,
            isAggregated: false);

        aggregateData.RaiseEvent(new AggregatedTelemetryAddedDomainEvent
        {
            SensorId = sensorId,
            EcosystemId = ecosystemId,
            Period = period,
            MaxValue = summary.MaxValue,
            MinValue = summary.MinValue,
            AvgValue = summary.AvgValue,
            Time = periodStart,
        });

        return Result<AggregateTelemetry>.Success(aggregateData);
    }
    public void MarkAsAggregated()
    {
        IsAggregated = true;

        IncrementVersion();
    }
}
