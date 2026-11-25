using BuildingBlocks.Domain.Results;
using Device.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Device.Infrastructure.BackgroundJobs;

[DisallowConcurrentExecution]
public sealed class RetryRelayCommandsJob(
    IRelayCommandRetryService retryService,
    ILogger<RetryRelayCommandsJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        Result<int> result = await retryService.ProcessActiveCommandsAsync(context.CancellationToken);
        if (result.IsFailure)
        {
            throw new JobExecutionException(result.Error.Message);
        }

        if (result.Value > 0)
        {
            logger.LogInformation(
                "RetryRelayCommandsJob processed {ProcessedCount} active relay commands.",
                result.Value);
        }
    }
}
