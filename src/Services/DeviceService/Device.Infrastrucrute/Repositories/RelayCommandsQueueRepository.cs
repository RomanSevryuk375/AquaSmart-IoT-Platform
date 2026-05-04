using Contracts.Enums;
using Device.Domain.Entities;
using Device.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Device.Infrastructure.Repositories;

public sealed class RelayCommandsQueueRepository(SystemDbContext dbContext)
    : BaseRepository<RelayCommandsQueueEntity>(dbContext), IRelayCommandsQueueRepository
{
    private const int maxAttemptCount = 3;

    public async Task<IReadOnlyList<RelayCommandsQueueEntity>> GetPendingByControllerIdAsync(
        Guid controllerId,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var retryThreshold = now.AddMinutes(-1);

        return await Context.RelayCommands
            .Where(x => x.ControllerId == controllerId)
            .Where(x => x.ExpireAt == null || x.ExpireAt > now)
            .Where(x => 
                x.Status == CommandStatusEnum.Pending ||
                (x.Status == CommandStatusEnum.Sent &&
                x.AttemptCount < maxAttemptCount &&
                x.ProcessedAt < retryThreshold))
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteCompletedAsync(
        CancellationToken cancellationToken)
    {
        await Context.RelayCommands
            .Where(x => x.Status == CommandStatusEnum.Completed 
                     || x.ExpireAt < DateTime.UtcNow)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
