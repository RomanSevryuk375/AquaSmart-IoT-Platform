namespace Device.Infrastructure.Persistence.Repositories;

public sealed class RelayCommandsQueueRepository(DeviceDbContext dbContext)
    : BaseRepository<RelayCommand>(dbContext), IRelayCommandsRepository
{
    private const int MaxAttemptCount = 3;
    private const int RetryCooldownMinutes = 1;

    public async Task<IReadOnlyList<RelayCommand>> GetPendingByControllerIdAsync(
        Guid controllerId,
        CancellationToken cancellationToken = default)
    {
        DateTime now = DateTime.UtcNow;
        DateTime retryThreshold = now.AddMinutes(-RetryCooldownMinutes);

        return await Context.RelayCommands
            .Where(x => x.ControllerId == controllerId)
            .Where(x => x.ExpireAt == null || x.ExpireAt > now)
            .Where(x =>
                x.Status == CommandStatus.Pending ||
                (x.Status == CommandStatus.Sent &&
                x.AttemptCount < MaxAttemptCount &&
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
