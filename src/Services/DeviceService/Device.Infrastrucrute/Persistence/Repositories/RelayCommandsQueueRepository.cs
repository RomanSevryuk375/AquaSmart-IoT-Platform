using Contracts.Enums;
using Device.Domain.Entities;
using Device.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Device.Infrastructure.Persistence.Repositories;

public sealed class RelayCommandsQueueRepository(SystemDbContext dbContext)
    : BaseRepository<RelayCommand>(dbContext), IRelayCommandsQueueRepository
{
    private const int MaxAttemptCount = 3;
    private const int RetryCooldownMinutes = 1;

    public async Task<IReadOnlyList<RelayCommand>> GetPendingByControllerIdAsync(
        Guid controllerId,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var retryThreshold = now.AddMinutes(-RetryCooldownMinutes);

        return await Context.RelayCommands
            .Where(x => x.ControllerId == controllerId)
            .Where(x => x.ExpireAt == null || x.ExpireAt > now)
            .Where(x => 
                x.Status == CommandStatusEnum.Pending ||
                (x.Status == CommandStatusEnum.Sent &&
                x.AttemptCount < MaxAttemptCount &&
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
