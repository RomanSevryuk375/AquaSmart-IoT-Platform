using Contracts.Results;
using MediatR;
using Quartz;
using Telemetry.Application.Features.BackgroundJobs.Commands.CompressToDays;

namespace Telemetry.Infrastructure.BackgroundJobs;

[DisallowConcurrentExecution]
public sealed class CompressRawDataToDaysJob(ISender sender) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        Result result = await sender.Send(new CompressToDaysCommand(), context.CancellationToken);
        if (result.IsFailure)
        {
            throw new JobExecutionException(result.Error.Message);
        }
    }
}
