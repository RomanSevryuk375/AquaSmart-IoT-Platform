using Contracts.Results;
using Quartz;
using Telemetry.Application.Interfaces;

namespace Telemetry.Infrastructure.BackgroundJobs;

public sealed class OutboxMessageProcessorJob(IOutboxMessageProcessorService service) : IJob
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
