using Contracts.Abstractions;
using Telemetry.Domain.Entities;
using Telemetry.Domain.SpecificationParams;

namespace Telemetry.Domain.Specifications;

public sealed class TelemetryAggregateFilterSpecification 
    : BaseSpecification<TelemetryAggregateEntity>
{
    public TelemetryAggregateFilterSpecification(
        TelemetryAggregateFilterParams @params)
        : base(data =>
            (!@params.SensorId.HasValue || data.SensorId == @params.SensorId.Value) &&
            (!@params.Period.HasValue || data.Period == @params.Period.Value) &&
            (!@params.From.HasValue || data.PeriodStart >= @params.From.Value) &&
            (!@params.To.HasValue || data.PeriodStart <= @params.To.Value))
    {
    }
}