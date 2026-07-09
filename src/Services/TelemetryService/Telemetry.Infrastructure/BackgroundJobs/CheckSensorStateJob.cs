using Contracts.Results;
using MediatR;
using Quartz;
using Telemetry.Application.Features.BackgroundJobs.Commands.CheckSensorState;

namespace Telemetry.Infrastructure.BackgroundJobs;

[DisallowConcurrentExecution]
public sealed class CheckSensorStateJob(ISender sender) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        Result result = await sender.Send(new CheckSensorStateCommand(), context.CancellationToken);
        if (result.IsFailure)
        {
            throw new JobExecutionException(result.Error.Message);
        }
    }
}
