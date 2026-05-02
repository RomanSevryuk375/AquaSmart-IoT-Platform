using Device.Application.Interfaces;
using Quartz;

namespace Device.Infrastructure.BackgroundJobs;

public class DeleteCompletedCommandsJob(
    IRelayCommandQueueService service) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await service.DeleteCompletedCommandsAsync(context.CancellationToken);
    }
}
