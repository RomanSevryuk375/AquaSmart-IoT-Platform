using Contracts.Results;
using MediatR;
using Quartz;
using Telemetry.Application.Features.BackgroundJobs.Commands.CompressToHours;

namespace Telemetry.Infrastructure.BackgroundJobs;

[DisallowConcurrentExecution]
public sealed class CompressRawDataToHoursJob(ISender sender) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        Result result = await sender.Send(new CompressToHoursCommand(), context.CancellationToken);
        if (result.IsFailure)
        {
            throw new JobExecutionException(result.Error.Message);
        }
    }
}
