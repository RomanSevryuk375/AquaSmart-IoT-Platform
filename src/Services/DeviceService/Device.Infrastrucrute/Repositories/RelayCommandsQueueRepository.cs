using Contracts.Enums;
using Device.Domain.Entities;
using Device.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Device.Infrastructure.Repositories;

public sealed class RelayCommandsQueueRepository(SystemDbContext dbContext)
    : BaseRepository<RelayCommandsQueueEntity>(dbContext), IRelayCommandsQueueRepository
{
    public async Task<IReadOnlyList<RelayCommandsQueueEntity>> GetPendingByControllerIdAsync(
        Guid controllerId,
        CancellationToken cancellationToken)
    {
        var retryThreshold = DateTime.UtcNow.AddMinutes(-1);
        var now = DateTime.UtcNow;

        return await Context.RelayCommands
            .Where(x => x.ControllerId == controllerId &&
                        (x.ExpireAt == null || x.ExpireAt > now) &&
                        (x.Status == CommandStatusEnum.Pending ||
                        (x.Status == CommandStatusEnum.Sent &&
                         x.AttemptCount < 3 &&
                         x.ProcessedAt < retryThreshold)))
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
