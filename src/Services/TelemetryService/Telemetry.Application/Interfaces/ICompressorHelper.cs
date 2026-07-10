using Contracts.Enums;
using Telemetry.Domain.ValueObjects;

namespace Telemetry.Application.Interfaces;

public interface ICompressorHelper
{
    public Task CreateAndSaveAggregatedTelemetryAsync(
        Guid sensorId,
        TelemetrySummary summary,
        DateTime from,
        PeriodType periodType,
        CancellationToken cancellationToken = default);
}
