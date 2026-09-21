using BuildingBlocks.Domain.Results;
using Control.Application.Features.Schedules.Commands.ProcessSchedules;
using MediatR;
using Quartz;

namespace Control.Infrastructure.BackgroundJobs;

[DisallowConcurrentExecution]
public sealed class ScheduleProcessJob(ISender sender) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        DateTime fireTime = (context.ScheduledFireTimeUtc ?? context.FireTimeUtc).UtcDateTime;

        Result result = await sender.Send(new ProcessSchedulesCommand(fireTime), context.CancellationToken);
        if (result.IsFailure)
        {
            throw new JobExecutionException(result.Error.Message);
        }
    }
}
