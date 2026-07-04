namespace Device.Infrastructure.Persistence.Repositories;

public sealed class RelayRepository(DeviceDbContext dbContext)
    : BaseRepository<Relay>(dbContext), IRelayRepository
{
    public async Task<bool> ExistsAsync(
        Guid relayId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Relays
            .AnyAsync(x => x.Id == relayId, cancellationToken);
    }

    public async Task<IReadOnlyList<Relay>> GetAllByControllerId(
        Guid controllerId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Relays
            .Where(x => x.ControllerId == controllerId)
            .ToListAsync(cancellationToken);
    }
}
