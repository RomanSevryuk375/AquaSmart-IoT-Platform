using Control.Domain.Entities;
using Control.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Control.Infrastructure.Persistence.Repositories;

public sealed class RelayRepository(ControlDbContext dbContext)
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

    public async Task<Relay?> GetByPowerSensorId(
        Guid powerSensorId,
        CancellationToken cancellationToken)
    {
        return await Context.Relays
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.PowerSensorId == powerSensorId, cancellationToken);
    }

    public async Task<IReadOnlyList<Relay>> GetManyByIds(
        IEnumerable<Guid> relayIds,
        CancellationToken cancellationToken)
    {
        return await Context.Relays.AsNoTracking()
            .Where(x => relayIds.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }
}
