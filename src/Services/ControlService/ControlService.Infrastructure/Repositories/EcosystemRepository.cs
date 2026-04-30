using Control.Domain.Entities;
using Control.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Control.Infrastructure.Repositories;

public sealed class EcosystemRepository(SystemDbContext dbContext)
    : BaseRepository<EcosystemEntity>(dbContext), IAquariumRepository
{
    public async Task<bool> ExistsAsync(
        Guid ecosystemId, 
        CancellationToken cancellationToken)
    {
        return await Context.Aquariums
            .AsNoTracking()
            .AnyAsync(x => x.Id == ecosystemId, cancellationToken);
    }

    public async Task<EcosystemEntity?> GetByControllerIdAsync(
        Guid controllerId, 
        CancellationToken cancellationToken)
    {
        return await Context.Aquariums
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ControllerId == controllerId, cancellationToken);
    }
}
