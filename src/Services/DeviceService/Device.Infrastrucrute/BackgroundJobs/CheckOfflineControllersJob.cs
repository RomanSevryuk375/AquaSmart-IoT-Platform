using Contracts.Results;
using Device.Application.Interfaces;
using Quartz;

namespace Device.Infrastructure.BackgroundJobs;

[DisallowConcurrentExecution]
public sealed class CheckOfflineControllersJob(
    IControllerOfflineCheckerService service) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        Result result = await service.CheckAndDisableControllerAsync(context.CancellationToken);
        if (result.IsFailure)
        {
            throw new JobExecutionException(result.Error.Message);
        }
    }
}
