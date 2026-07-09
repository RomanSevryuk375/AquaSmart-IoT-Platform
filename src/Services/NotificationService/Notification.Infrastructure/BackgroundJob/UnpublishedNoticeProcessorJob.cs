using Contracts.Results;
using MediatR;
using Notification.Application.Features.BackgroundJobs.Commands.ProcessUnpublishedNotices;
using Quartz;

namespace Notification.Infrastructure.BackgroundJob;

public sealed class UnpublishedNoticeProcessorJob(ISender sender) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        Result result = await sender.Send(new ProcessUnpublishedNoticesCommand(), context.CancellationToken);
        if (result.IsFailure)
        {
            throw new JobExecutionException(result.Error.Message);
        }
    }
}
