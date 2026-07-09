using Contracts.Enums;
using Contracts.Results;
using MediatR;
using Telemetry.Application.Interfaces;
using Telemetry.Domain.Interfaces;
using Telemetry.Domain.ValueObjects;

namespace Telemetry.Application.Features.BackgroundJobs.Commands.CompressToHours;

public sealed class CompressToHoursHandler(
    ITelemetryAggregateDataRepository telemetryAggregate,
    ICompressorHelper compressorHelper) : IRequestHandler<CompressToHoursCommand, Result>
{
    private const int HourlyInterval = -1;

    public async Task<Result> Handle(CompressToHoursCommand request, CancellationToken cancellationToken)
    {
        var to = new DateTime(
            DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day,
            DateTime.UtcNow.Hour, 0, 0, DateTimeKind.Utc);
        DateTime from = to.AddHours(HourlyInterval);

        IReadOnlyDictionary<Guid, TelemetrySummary>? data = await telemetryAggregate.GetSummaryForPeriodAsync(
            PeriodType.Minute, from, to, cancellationToken);
        if (data is null)
        {
            return Result.Success();
        }

        foreach (KeyValuePair<Guid, TelemetrySummary> kvp in data)
        {
            await compressorHelper.CreateAndSaveAggregatedTelemetryAsync(
                kvp.Key, kvp.Value, from, PeriodType.Hourly, cancellationToken);
        }

        var sensorIds = data.Keys.ToList();
        await telemetryAggregate.MarkAsAggregatedAsync(sensorIds, from, to, cancellationToken);
        await compressorHelper.NotifyClientsAsync(data, from, PeriodType.Hourly, cancellationToken);

        return Result.Success();
    }
}
