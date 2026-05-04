using Device.Application.Interfaces;
using Quartz;

namespace Device.Infrastructure.BackgroundJobs;

[DisallowConcurrentExecution]
public class CheckOfflineControllersJob(
    IControllerOfflineCheckerService service) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var result = await service.CheckAndDisableController(context.CancellationToken);

        if (result.IsFailure)
        {
            throw new ArgumentException(result.Error.Message);
        }
    }
}
