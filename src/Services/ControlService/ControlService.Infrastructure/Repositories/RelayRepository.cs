using Control.Domain.Entities;
using Control.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Control.Infrastructure.Repositories;

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

    public async Task<RelayEntity?> GetByPowerSensorId(
        Guid powerSensorId,
        CancellationToken cancellationToken)
    {
        return await Context.Relays
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.PowerSensorId == powerSensorId, cancellationToken);
    }
}
