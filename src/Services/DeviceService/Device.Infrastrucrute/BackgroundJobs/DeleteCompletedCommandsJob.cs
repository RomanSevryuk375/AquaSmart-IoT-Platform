using Device.Application.Interfaces;
using Quartz;

namespace Device.Infrastructure.BackgroundJobs;

public class DeleteCompletedCommandsJob(
    IRelayCommandQueueService service) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var result = await service.DeleteCompletedCommandsAsync(context.CancellationToken);

        if (result.IsFailure)
        {
            throw new ArgumentException(result.Error.Message);
        }
    }
}
