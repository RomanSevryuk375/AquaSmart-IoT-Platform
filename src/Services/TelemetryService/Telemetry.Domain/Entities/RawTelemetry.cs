using Contracts.Abstractions;
using Contracts.Results;

namespace Telemetry.Domain.Entities;

public sealed class RawTelemetry : AggregateRoot, IEntity
{
    private RawTelemetry(
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

#pragma warning disable CS8618 
    private RawTelemetry() { }
#pragma warning restore CS8618 

    public Guid Id { get; private set; }
    public Guid SensorId { get; private set; }
    public double Value { get; private set; }
    public string ExternalMessageId { get; private set; }
    public DateTime RecordedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsAggregated { get; private set; }

    public static Result<RawTelemetry> Create(
        Guid id, Guid sensorId,
        double value, string externalMessageId, DateTime recordedAt)
    {
        var errors = new List<string>();

        if (recordedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("recordedAt cannot be in the future.");
        }

        if (string.IsNullOrWhiteSpace(externalMessageId))
        {
            errors.Add("externalMessageId must not be empty.");
        }

        if (errors.Count > 0)
        {
            return Result<RawTelemetry>.Failure(Error.Validation<RawTelemetry>(
                    string.Join("; ", errors)));
        }

        var telemetryData = new RawTelemetry(
            id, sensorId,
            value, externalMessageId, recordedAt,
            createdAt: DateTime.UtcNow, isAggregated: false);

        return Result<RawTelemetry>.Success(telemetryData);
    }

    public void MarkAsAggregated()
    {
        IsAggregated = true;

        IncrementVersion();
    }
}
