using IdentityService.Application.Interfaces;
using Quartz;

namespace IdentityService.Infrastructure.BackgroundJobs;

public class IncorrectTokenCheckerJob(
    IIncorrectTokenChecker checker) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await checker.CheckAndDeleteAsync(context.CancellationToken);
    }
}
