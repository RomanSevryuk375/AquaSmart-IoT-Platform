using Contracts.Abstractions;

namespace Telemetry.Domain.Entities;

public sealed class TelemetryRawEntity : IEntity
{
    private TelemetryRawEntity(
        Guid id,
        Guid sensorId,
        double value,
        string externalMessageId,
        DateTime recordedAt,
        DateTime createdAt,
        bool isAggregated)
    {
        Id = id;
        SensorId = sensorId;
        Value = value;
        ExternalMessageId = externalMessageId;
        RecordedAt = recordedAt;
        CreatedAt = createdAt;
        IsAggregated = isAggregated;
    }

    public Guid Id { get; private set; }
    public Guid SensorId { get; private set; }
    public double Value { get; private set; }
    public string ExternalMessageId { get; private set; }
    public DateTime RecordedAt { get; private set; }
    public DateTime CreatedAt { get; private set; } 
    public bool IsAggregated { get; private set; } 

    public static (TelemetryRawEntity? telemetryData, List<string> errors) Create(
        Guid sensorId,
        double value,
        string externalMessageId,
        DateTime recordedAt)
    {
        var errors = new List<string>();

        if (sensorId == Guid.Empty)
        {
            errors.Add("sensorId must not be empty.");
        }

        if (recordedAt > DateTime.UtcNow.AddMinutes(1))
        {
            errors.Add("recordedAt cannot be in the future.");
        }

        if (string.IsNullOrWhiteSpace(externalMessageId))
        {
            errors.Add("externalMessageId must not be empty.");
        }

        if (errors.Count > 0)
        {
            return (null, errors);
        }

        var telemetryData = new TelemetryRawEntity(
            Guid.NewGuid(),
            sensorId,
            value, 
            externalMessageId,
            recordedAt,
            DateTime.UtcNow,
            false);

        return (telemetryData, errors);
    }

    public void MarkAsAggregated()
    {
        IsAggregated = true;
    }
}
