using Contracts.Abstractions;
using Telemetry.Domain.Entities;
using Telemetry.Domain.SpecificationParams;

namespace Telemetry.Domain.Specifications;

public sealed class TelemetryFilterSpecification 
    : BaseSpecification<TelemetryRawEntity>
{
    public TelemetryFilterSpecification(
        TelemetryFilterParams @params)
        : base (data =>
            (!@params.SensorId.HasValue || data.SensorId == @params.SensorId) &&
            (!@params.From.HasValue || data.RecordedAt >= @params.From.Value) &&
            (!@params.To.HasValue || data.RecordedAt <= @params.To.Value))
    {
    }
}
