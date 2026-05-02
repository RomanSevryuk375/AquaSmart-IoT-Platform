using IdentityService.Application.Interfaces;
using Quartz;

namespace IdentityService.Infrastructure.BackgroundJobs;

public class SubscriptionExpiredCheckerJob(
    ISubscriptionExpiredChecker checker) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await checker.CheckAsync(context.CancellationToken);
    }
}
