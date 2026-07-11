using Contracts.Abstractions;
using Contracts.Enums;

namespace Telemetry.Domain.Events;

public sealed record AggregatedTelemetryAddedDomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid SensorId { get; init; }
    public Guid EcosystemId { get; init; }
    public PeriodType Period { get; init; }
    public double MaxValue { get; init; }
    public double MinValue { get; init; }
    public double AvgValue { get; init; }
    public DateTime Time { get; init; }
}
