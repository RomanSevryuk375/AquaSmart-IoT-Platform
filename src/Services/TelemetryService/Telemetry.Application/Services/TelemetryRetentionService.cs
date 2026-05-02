using Contracts.Enums;
using Telemetry.Application.Interfaces;
using Telemetry.Domain.Interfaces;

namespace Telemetry.Application.Services;

public sealed class TelemetryRetentionService(
    ITelemetryRawDataRepository rawRepo,
    ITelemetryAggregateDataRepository aggregateRepo) : ITelemetryRetentionService
{
    public async Task CleanUpOldDataAsync(CancellationToken cancellationToken)
    {
        var rawThreshold = DateTime.UtcNow.AddHours(-24);
        await rawRepo
            .DeleteOldRawDataAsync(rawThreshold, cancellationToken);

        var minuteThreshold = DateTime.UtcNow.AddDays(-7);
        await aggregateRepo
            .DeleteOldRawDataAsync(PeriodTypeEnum.Minute, minuteThreshold, cancellationToken);

        var hourlyThreshold = DateTime.UtcNow.AddDays(-90);
        await aggregateRepo
            .DeleteOldRawDataAsync(PeriodTypeEnum.Hourly, hourlyThreshold, cancellationToken);
    }
}
