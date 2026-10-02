using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Domain.Results;
using MediatR;
using Telemetry.Application.Interfaces;
using Telemetry.Domain.Interfaces;
using Telemetry.Domain.ValueObjects;

namespace Telemetry.Application.Features.BackgroundJobs.Commands.CompressToHours;

public sealed class CompressToHoursHandler(
    ITelemetryAggregateDataRepository telemetryAggregate,
    ICompressorHelper compressorHelper) : IRequestHandler<CompressToHoursCommand, Result>
{
    private const int MaxBatchHours = 48;

    public async Task<Result> Handle(CompressToHoursCommand request, CancellationToken cancellationToken)
    {
        var ceilingUtc = new DateTime(
            DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day,
            DateTime.UtcNow.Hour, 0, 0, DateTimeKind.Utc);

        IReadOnlyList<DateTime> windows = await telemetryAggregate.GetUnaggregatedWindowsAsync(
            PeriodType.Minute, "hour", ceilingUtc, MaxBatchHours, cancellationToken);

        if (windows.Count == 0)
        {
            return Result.Success();
        }

        foreach (DateTime windowStart in windows)
        {
            DateTime windowEnd = windowStart.AddHours(1);

            IReadOnlyDictionary<Guid, TelemetrySummary> data = await telemetryAggregate.GetSummaryForPeriodAsync(
                PeriodType.Minute, windowStart, windowEnd, cancellationToken);

            if (data.Count > 0)
            {
                foreach (KeyValuePair<Guid, TelemetrySummary> kvp in data)
                {
                    await compressorHelper.CreateAndSaveAggregatedTelemetryAsync(
                        kvp.Key, kvp.Value, windowStart, PeriodType.Hourly, cancellationToken);
                }

                var sensorIds = data.Keys.ToList();
                await telemetryAggregate.MarkAsAggregatedAsync(
                    sensorIds, PeriodType.Minute, windowStart, windowEnd, cancellationToken);
            }
        }

        return Result.Success();
    }
}
