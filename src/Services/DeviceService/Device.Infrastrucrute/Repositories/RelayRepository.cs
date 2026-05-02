using Device.Domain.Entities;
using Device.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Device.Infrastructure.Repositories;

public sealed class RelayRepository(SystemDbContext dbContext)
    : BaseRepository<RelayEntity>(dbContext), IRelayRepository
{
    public async Task<bool> ExistsAsync(
        Guid relayId, 
        CancellationToken cancellationToken)
    {
        return await Context.Relays
            .AsNoTracking()
            .AnyAsync(x => x.Id == relayId, cancellationToken);
    }

    public async Task<IReadOnlyList<RelayEntity>> GetAllByControllerId(
        Guid controllerId,
        CancellationToken cancellationToken)
    {
        return await Context.Relays
            .AsNoTracking()
            .Where(x => x.ControllerId == controllerId)
            .ToListAsync(cancellationToken);
    }
}
