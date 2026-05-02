using Quartz;
using Telemetry.Application.Interfaces;

namespace Telemetry.Infrastructure.BackgroundJobs;

[DisallowConcurrentExecution]
public sealed class CompressRawDataToMinutesJob(
    ICompressorService service) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await service
            .CompressToMinutesAsync(context.CancellationToken);
    }
}
