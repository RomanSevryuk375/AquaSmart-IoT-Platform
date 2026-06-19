namespace Device.Infrastructure.Persistence.Repositories;

public sealed class RelayRepository(SystemDbContext dbContext)
    : BaseRepository<Relay>(dbContext), IRelayRepository
{
    public async Task<bool> ExistsAsync(
        Guid relayId, 
        CancellationToken cancellationToken)
    {
        return await Context.Relays
            .AsNoTracking()
            .AnyAsync(x => x.Id == relayId, cancellationToken);
    }

    public async Task<IReadOnlyList<Relay>> GetAllByControllerId(
        Guid controllerId,
        CancellationToken cancellationToken)
    {
        return await Context.Relays
            .AsNoTracking()
            .Where(x => x.ControllerId == controllerId)
            .ToListAsync(cancellationToken);
    }
}
