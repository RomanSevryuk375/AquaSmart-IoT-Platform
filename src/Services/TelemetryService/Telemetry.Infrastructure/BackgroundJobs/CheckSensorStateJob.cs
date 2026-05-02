using Quartz;
using Telemetry.Application.Interfaces;

namespace Telemetry.Infrastructure.BackgroundJobs;

[DisallowConcurrentExecution]
public sealed class CheckSensorStateJob(
    ISensorStateCheckerService service) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await service
            .CheckStateAndNotify(context.CancellationToken);
    }
}
