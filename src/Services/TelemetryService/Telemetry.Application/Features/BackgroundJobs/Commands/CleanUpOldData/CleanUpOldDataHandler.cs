using Contracts.Enums;
using Contracts.Results;
using MediatR;
using Microsoft.Extensions.Options;
using Telemetry.Application.Extensions;
using Telemetry.Domain.Interfaces;

namespace Telemetry.Application.Features.BackgroundJobs.Commands.CleanUpOldData;

public sealed class CleanUpOldDataHandler(
    ITelemetryRawDataRepository rawRepo,
    ITelemetryAggregateDataRepository aggregateRepo,
    IOptions<TelemetrySettings> telemetrySettings) : IRequestHandler<CleanUpOldDataCommand, Result>
{
    public async Task<Result> Handle(CleanUpOldDataCommand request, CancellationToken cancellationToken)
    {
        DateTime rawThreshold = DateTime.UtcNow
            .AddHours(telemetrySettings.Value.MaxLiveTimeForRawDataInHours);
        await rawRepo.DeleteOldRawDataAsync(rawThreshold, cancellationToken);

        DateTime minuteThreshold = DateTime.UtcNow
            .AddDays(telemetrySettings.Value.MaxLiveTimeForMinutesDataInDayes);
        await aggregateRepo.DeleteOldRawDataAsync(PeriodType.Minute, minuteThreshold, cancellationToken);

        DateTime hourlyThreshold = DateTime.UtcNow
            .AddDays(telemetrySettings.Value.MaxLiveTimeForHourseDataInDayes);
        await aggregateRepo.DeleteOldRawDataAsync(PeriodType.Hourly, hourlyThreshold, cancellationToken);

        return Result.Success();
    }
}
