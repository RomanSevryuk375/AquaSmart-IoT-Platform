using Contracts.Abstractions;

namespace Telemetry.Domain.Events;

public sealed record RawTelemetryAddedDomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid SensorId { get; init; }
    public Guid EcosystemId { get; init; }
    public double Value { get; init; }
    public DateTime RecordedAt { get; init; }
    public string ExternalMessageId { get; init; } = string.Empty;
}
