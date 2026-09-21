using BuildingBlocks.Domain.Results;
using IdentityService.Application.Features.BackgroundJobs.Commands.CleanIncorrectTokens;
using MediatR;
using Quartz;

namespace IdentityService.Infrastructure.BackgroundJobs;

[DisallowConcurrentExecution]
public class IncorrectTokenCheckerJob(
    ISender sender) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        Result result = await sender.Send(new CleanIncorrectTokensCommand(), context.CancellationToken);
        if (result.IsFailure)
        {
            throw new JobExecutionException(result.Error.Message);
        }
    }
}
