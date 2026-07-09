using Contracts.Enums;
using Contracts.Results;
using MediatR;
using Telemetry.Application.Interfaces;
using Telemetry.Domain.Interfaces;
using Telemetry.Domain.ValueObjects;

namespace Telemetry.Application.Features.BackgroundJobs.Commands.CompressToMinutes;

public sealed class CompressToMinutesHandler(
    ITelemetryRawDataRepository telemetryRaw,
    ICompressorHelper compressorHelper) : IRequestHandler<CompressToMinutesCommand, Result>
{
    private const int MinuteInterval = -1;

    public async Task<Result> Handle(CompressToMinutesCommand request, CancellationToken cancellationToken)
    {
        var to = new DateTime(
            DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day,
            DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, 0, DateTimeKind.Utc);
        DateTime from = to.AddMinutes(MinuteInterval);

        IReadOnlyDictionary<Guid, TelemetrySummary> data =
            await telemetryRaw.GetSummaryForPeriodAsync(from, to, cancellationToken);
        if (data.Count == 0)
        {
            return Result.Success();
        }

        foreach (KeyValuePair<Guid, TelemetrySummary> kvp in data)
        {
            await compressorHelper.CreateAndSaveAggregatedTelemetryAsync(
                kvp.Key, kvp.Value, from, PeriodType.Minute, cancellationToken);
        }

        var sensorIds = data.Keys.ToList();
        await telemetryRaw.MarkAsAggregatedAsync(sensorIds, from, to, cancellationToken);
        await compressorHelper.NotifyClientsAsync(data, from, PeriodType.Minute, cancellationToken);

        return Result.Success();
    }
}
