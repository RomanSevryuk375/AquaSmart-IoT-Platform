using Quartz;
using Telemetry.Application.Interfaces;

namespace Telemetry.Infrastructure.BackgroundJobs;

public class CleanUpOldDataJob(
    ITelemetryRetentionService service) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await service
            .CleanUpOldDataAsync(context.CancellationToken);
    }
}
