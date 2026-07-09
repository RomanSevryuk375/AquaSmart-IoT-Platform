using Contracts.Results;
using IdentityService.Application.Features.BackgroundJobs.Commands.ProcessExpiredSubscriptions;
using MediatR;
using Quartz;

namespace IdentityService.Infrastructure.BackgroundJobs;

public class SubscriptionExpiredCheckerJob(
    ISender sender) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        Result result = await sender.Send(new ProcessExpiredSubscriptionsCommand(), context.CancellationToken);

        if (result.IsFailure)
        {
            throw new JobExecutionException(result.Error.Message);
        }
    }
}
