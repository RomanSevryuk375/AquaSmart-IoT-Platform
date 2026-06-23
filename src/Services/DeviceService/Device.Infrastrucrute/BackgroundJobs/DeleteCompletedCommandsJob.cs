using Contracts.Results;
using Device.Application.Features.RelayCommands.Command.DeleteCompleted;
using MediatR;
using Quartz;

namespace Device.Infrastructure.BackgroundJobs;

[DisallowConcurrentExecution]
public sealed class DeleteCompletedCommandsJob(
    ISender sender) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        Result result = await sender.Send(new DeleteCompletedCommand(), context.CancellationToken);
        if (result.IsFailure)
        {
            throw new JobExecutionException(result.Error.Message);
        }
    }
}
