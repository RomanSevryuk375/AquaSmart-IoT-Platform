using Contracts.Results;
using MediatR;
using Quartz;
using Telemetry.Application.Features.BackgroundJobs.Commands.CleanUpOldData;

namespace Telemetry.Infrastructure.BackgroundJobs;

public class CleanUpOldDataJob(ISender sender) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        Result result = await sender.Send(new CleanUpOldDataCommand(), context.CancellationToken);
        if (result.IsFailure)
        {
            throw new JobExecutionException(result.Error.Message);
        }
    }
}
