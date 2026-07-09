using Contracts.Results;
using Quartz;

namespace Device.Infrastructure.BackgroundJobs;

[DisallowConcurrentExecution]
public sealed class OutboxMessageProcessorJob(
    OutboxMessageProcessorService service) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        Result result = await service.ProcessAsync(context.CancellationToken);
        if (result.IsFailure)
        {
            throw new JobExecutionException(result.Error.Message);
        }
    }
}
