using Control.Application.Interfaces;
using Quartz;

namespace Control.Infrastructure.BackgroundJobs;

public sealed class ScheduleProcessJob(
    IScheduleProcessor scheduleProcessor) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var result = await scheduleProcessor.ProcessAsync(context.CancellationToken);

        if (result.IsFailure)
        {
            throw new JobExecutionException(result.Error.Message);
        }
    }
}
