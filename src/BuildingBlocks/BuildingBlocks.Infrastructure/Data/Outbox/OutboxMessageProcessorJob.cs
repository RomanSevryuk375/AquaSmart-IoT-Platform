using BuildingBlocks.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace BuildingBlocks.Infrastructure.Data.Outbox;

[DisallowConcurrentExecution]
public sealed class OutboxMessageProcessorJob<TDbContext>(OutboxMessageProcessorService<TDbContext> service)
    : IJob where TDbContext : DbContext
{
    public async Task Execute(IJobExecutionContext context)
    {
        Result result = await service.ProcessAsync(context.CancellationToken);

        if (result.IsFailure)
        {
            throw new JobExecutionException(result.Error.Message);
        }
    }
}
