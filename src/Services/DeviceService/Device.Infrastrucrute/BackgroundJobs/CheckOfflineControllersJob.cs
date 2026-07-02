using Contracts.Results;
using Device.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Device.Infrastructure.BackgroundJobs;

[DisallowConcurrentExecution]
public sealed class CheckOfflineControllersJob(
    IControllerOfflineCheckerService service,
    ILogger<CheckOfflineControllersJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        Result<int> result = await service.CheckAndDisableControllerAsync(context.CancellationToken);
        if (result.IsFailure)
        {
            throw new JobExecutionException(result.Error.Message);
        }

        logger.LogInformation("Successfully checked controllers. Marked {OfflineControllersCount} controllers offline.", result.Value);
    }
}
