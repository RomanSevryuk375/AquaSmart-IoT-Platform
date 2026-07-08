using Contracts.Results;
using MediatR;
using Notification.Application.Features.BackgroundJobs.Commands.ProcessReminders;
using Quartz;

namespace Notification.Infrastructure.BackgroundJob;

public sealed class ReminderCheckerJob(ISender sender) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        Result result = await sender.Send(new ProcessRemindersCommand(), context.CancellationToken);
        if (result.IsFailure)
        {
            throw new JobExecutionException(result.Error.Message);
        }
    }
}
