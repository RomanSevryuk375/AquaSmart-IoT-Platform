using Contracts.Enums;

namespace Telemetry.Domain.SpecificationParams;

public sealed record TelemetryAggregateFilterParams
{
    public Guid? SensorId { get; init; }
    public PeriodTypeEnum? Period { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
}
