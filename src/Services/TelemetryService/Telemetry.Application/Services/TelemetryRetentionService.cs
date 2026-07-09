using Contracts.Enums;
using Microsoft.Extensions.Options;
using Telemetry.Application.Extensions;
using Telemetry.Application.Interfaces;
using Telemetry.Domain.Interfaces;

namespace Telemetry.Application.Services;

public sealed class TelemetryRetentionService(
    ITelemetryRawDataRepository rawRepo,
    ITelemetryAggregateDataRepository aggregateRepo,
    IOptions<TelemetrySettings> telemetrySettings) : ITelemetryRetentionService
{
    public async Task CleanUpOldDataAsync(CancellationToken cancellationToken)
    {
        DateTime rawThreshold = DateTime.UtcNow
            .AddHours(telemetrySettings.Value.MaxLiveTimeForRawDataInHours);
        await rawRepo
            .DeleteOldRawDataAsync(rawThreshold, cancellationToken);

        DateTime minuteThreshold = DateTime.UtcNow
            .AddDays(telemetrySettings.Value.MaxLiveTimeForMinutesDataInDayes);
        await aggregateRepo
            .DeleteOldRawDataAsync(PeriodType.Minute, minuteThreshold, cancellationToken);

        DateTime hourlyThreshold = DateTime.UtcNow
            .AddDays(telemetrySettings.Value.MaxLiveTimeForHourseDataInDayes);
        await aggregateRepo
            .DeleteOldRawDataAsync(PeriodType.Hourly, hourlyThreshold, cancellationToken);
    }
}
