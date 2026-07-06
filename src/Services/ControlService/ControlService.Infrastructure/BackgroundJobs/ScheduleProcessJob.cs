using Contracts.Results;
using Control.Application.Features.Schedules.Commands.ProcessSchedules;
using MediatR;
using Quartz;

namespace Control.Infrastructure.BackgroundJobs;

public sealed class ScheduleProcessJob(ISender sender) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        Result result = await sender.Send(new ProcessSchedulesCommand(), context.CancellationToken);

        if (result.IsFailure)
        {
            throw new JobExecutionException(result.Error.Message);
        }
    }
}
