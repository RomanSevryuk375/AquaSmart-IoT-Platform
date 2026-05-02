using Quartz;
using Telemetry.Application.Interfaces;

namespace Telemetry.Infrastructure.BackgroundJobs;

[DisallowConcurrentExecution]
public sealed class CompressRawDataToDaysJob(
    ICompressorService service) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await service
            .CompressToDaysAsync(context.CancellationToken);
    }
}
