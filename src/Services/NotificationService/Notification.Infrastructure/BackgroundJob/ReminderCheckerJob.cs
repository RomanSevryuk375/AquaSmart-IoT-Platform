using Notification.Application.Interfaces;
using Quartz;

namespace Notification.Infrastructure.BackgroundJob;

public sealed class ReminderCheckerJob(
    IReminderProcessor processor) : IJob
{
    public async Task Execute(IJobExecutionContext context) => await processor.CheckAsync(context.CancellationToken);
}
