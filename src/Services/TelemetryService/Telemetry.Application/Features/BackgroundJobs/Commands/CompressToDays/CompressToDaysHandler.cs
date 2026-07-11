using Contracts.Enums;
using Contracts.Results;
using MediatR;
using Telemetry.Application.Interfaces;
using Telemetry.Domain.Interfaces;
using Telemetry.Domain.ValueObjects;

namespace Telemetry.Application.Features.BackgroundJobs.Commands.CompressToDays;

public sealed class CompressToDaysHandler(
    ITelemetryAggregateDataRepository telemetryAggregate,
    ICompressorHelper compressorHelper) : IRequestHandler<CompressToDaysCommand, Result>
{
    private const int DailyInterval = -24;
    public async Task<Result> Handle(CompressToDaysCommand request, CancellationToken cancellationToken)
    {
        var to = new DateTime(
            DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day,
            0, 0, 0, DateTimeKind.Utc);
        DateTime from = to.AddHours(DailyInterval);

        IReadOnlyDictionary<Guid, TelemetrySummary> data = await telemetryAggregate.GetSummaryForPeriodAsync(
            PeriodType.Hourly, from, to, cancellationToken);
        if (data.Count == 0)
        {
            return Result.Success();
        }

        foreach (KeyValuePair<Guid, TelemetrySummary> kvp in data)
        {
            await compressorHelper.CreateAndSaveAggregatedTelemetryAsync(
                kvp.Key, kvp.Value, from, PeriodType.Daily, cancellationToken);
        }

        var sensorIds = data.Keys.ToList();
        await telemetryAggregate.MarkAsAggregatedAsync(sensorIds, from, to, cancellationToken);

        return Result.Success();
    }
}
