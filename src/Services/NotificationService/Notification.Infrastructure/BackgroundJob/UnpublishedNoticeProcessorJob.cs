using Notification.Application.Services;
using Quartz;

namespace Notification.Infrastructure.BackgroundJob;

public sealed class UnpublishedNoticeProcessorJob(
    IUnpublishedNoticeProcessor processor) : IJob
{
    public async Task Execute(IJobExecutionContext context) => await processor.ProcessAsync(context.CancellationToken);
}
