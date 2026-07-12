namespace Device.Infrastructure.Persistence.Repositories;

public sealed class RelayCommandsQueueRepository(DeviceDbContext dbContext)
    : BaseRepository<RelayCommand>(dbContext), IRelayCommandsRepository
{
    public async Task<IReadOnlyList<RelayCommand>> GetPendingByControllerIdAsync(
        Guid controllerId,
        CancellationToken cancellationToken = default)
    {
        DateTime now = DateTime.UtcNow;
        DateTime retryThreshold = now.AddMinutes(-RelayCommandConstants.RetryCooldownMinutes);

        return await Context.RelayCommands
            .Where(x => x.ControllerId == controllerId)
            .Where(x => x.ExpireAt == null || x.ExpireAt > now)
            .Where(x =>
                x.Status == CommandStatus.Pending ||
                (x.Status == CommandStatus.Sent &&
                x.AttemptCount < RelayCommandConstants.MaxAttemptCount &&
                x.ProcessedAt < retryThreshold))
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> DeleteCompletedAsync(
        CancellationToken cancellationToken = default)
    {
        return await Context.RelayCommands
            .Where(x => x.Status == CommandStatus.Completed
                     || x.ExpireAt < DateTime.UtcNow)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
