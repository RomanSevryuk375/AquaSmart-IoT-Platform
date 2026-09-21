using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Domain.Results;
using MediatR;
using Telemetry.Application.Interfaces;
using Telemetry.Domain.Interfaces;
using Telemetry.Domain.ValueObjects;

namespace Telemetry.Application.Features.BackgroundJobs.Commands.CompressToMinutes;

public sealed class CompressToMinutesHandler(
    ITelemetryRawDataRepository telemetryRaw,
    ICompressorHelper compressorHelper) : IRequestHandler<CompressToMinutesCommand, Result>
{
    private const int MaxBatchWindows = 120;

    public async Task<Result> Handle(CompressToMinutesCommand request, CancellationToken cancellationToken)
    {
        var ceilingUtc = new DateTime(
            DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day,
            DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, 0, DateTimeKind.Utc);

        IReadOnlyList<DateTime> windows = await telemetryRaw.GetUnaggregatedMinuteWindowsAsync(
            ceilingUtc, MaxBatchWindows, cancellationToken);

        if (windows.Count == 0)
        {
            return Result.Success();
        }

        foreach (DateTime windowStart in windows)
        {
            DateTime windowEnd = windowStart.AddMinutes(1);

            IReadOnlyDictionary<Guid, TelemetrySummary> data =
                await telemetryRaw.GetSummaryForPeriodAsync(windowStart, windowEnd, cancellationToken);

            if (data.Count > 0)
            {
                foreach (KeyValuePair<Guid, TelemetrySummary> kvp in data)
                {
                    await compressorHelper.CreateAndSaveAggregatedTelemetryAsync(
                        kvp.Key, kvp.Value, windowStart, PeriodType.Minute, cancellationToken);
                }

                var sensorIds = data.Keys.ToList();
                await telemetryRaw.MarkAsAggregatedAsync(sensorIds, windowStart, windowEnd, cancellationToken);
            }
        }

        return Result.Success();
    }
}
