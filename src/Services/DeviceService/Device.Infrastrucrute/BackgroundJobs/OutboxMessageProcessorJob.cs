using Device.Application.Interfaces;
using Quartz;

namespace Device.Infrastructure.BackgroundJobs;

public sealed class OutboxMessageProcessorJob(
    IOutboxMessageProcessorService service) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var result = await service.ProcessAsync(context.CancellationToken);

        if (result.IsFailure)
        {
            throw new JobExecutionException(result.Error.Message);
        }
    }
}
