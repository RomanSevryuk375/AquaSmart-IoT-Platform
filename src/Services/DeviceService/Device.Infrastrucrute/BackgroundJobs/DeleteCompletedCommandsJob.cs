using Contracts.Results;
using Device.Application.Features.RelayCommands.Command.DeleteCompleted;
using MediatR;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Device.Infrastructure.BackgroundJobs;

[DisallowConcurrentExecution]
public sealed class DeleteCompletedCommandsJob(
    ISender sender,
    ILogger<DeleteCompletedCommandsJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        Result<int> result = await sender.Send(new DeleteCompletedCommand(), context.CancellationToken);
        if (result.IsFailure)
        {
            throw new JobExecutionException(result.Error.Message);
        }

        logger.LogInformation("Deleted {DeletedCommandsCount} expired/completed commands.", result.Value);
    }
}
