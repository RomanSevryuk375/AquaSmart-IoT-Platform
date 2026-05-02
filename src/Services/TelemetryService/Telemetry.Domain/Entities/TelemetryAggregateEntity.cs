using Contracts.Abstractions;
using Contracts.Enums;

namespace Telemetry.Domain.Entities;

public sealed class TelemetryAggregateEntity : IEntity
{
    private TelemetryAggregateEntity(
        Guid id,
        Guid sensorId,
        DateTime periodStart,
        PeriodTypeEnum period,
        double minValue,
        double maxValue,
        double avgValue,
        int dataPointsCount,
        DateTime createdAt,
        bool isAggregated)
    {
        Id = id;
        SensorId = sensorId;
        PeriodStart = periodStart;
        Period = period;
        MinValue = minValue;
        MaxValue = maxValue;
        AvgValue = avgValue;
        DataPointsCount = dataPointsCount;
        CreatedAt = createdAt;
        IsAggregated = isAggregated;
    }

    public Guid Id { get; private set; }
    public Guid SensorId { get; private set; }
    public DateTime PeriodStart { get; private set; }
    public PeriodTypeEnum Period { get; private set; }
    public double MinValue { get; private set; }
    public double MaxValue { get; private set; }
    public double AvgValue { get; private set; }
    public int DataPointsCount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsAggregated { get; private set; }


    public static (TelemetryAggregateEntity? aggregateData, List<string>? errors) Create(
        Guid sensorId,
        DateTime periodStart,
        PeriodTypeEnum period,
        double minValue,
        double maxValue,
        double avgValue,
        int dataPointsCount)
    {
        var errors = new List<string>();

        if (sensorId == Guid.Empty)
        {
            errors.Add("Sensor id must not be empty.");
        }

        if (dataPointsCount <= 0)
        {
            errors.Add("Data points count must be greater than zero.");
        }

        if (minValue > maxValue)
        {
            errors.Add("MinValue cannot be greater than MaxValue.");
        }

        if (avgValue < minValue || avgValue > maxValue)
        {
            errors.Add("AvgValue must be between MinValue and MaxValue.");
        }

        if (errors.Count > 0)
        {
            return (null, errors);
        }

        var aggregateData = new TelemetryAggregateEntity(
            Guid.NewGuid(),
            sensorId,
            periodStart,
            period,
            minValue,
            maxValue,
            avgValue,
            dataPointsCount,
            DateTime.UtcNow,
            false);

        return (aggregateData, errors);
    }
    public void MarkAsAggregated()
    {
        IsAggregated = true;
    }
}