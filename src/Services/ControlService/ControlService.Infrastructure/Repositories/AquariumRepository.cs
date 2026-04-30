using Control.Domain.Entities;
using Control.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Control.Infrastructure.Repositories;

public class AquariumRepository(SystemDbContext dbContext)
    : BaseRepository<EcosystemEntity>(dbContext), IAquariumRepository
{
    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        return await Context.Aquariums
            .AsNoTracking()
            .AnyAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<EcosystemEntity?> GetByControllerIdAsync(
        Guid controllerId, CancellationToken cancellationToken)
    {
        return await Context.Aquariums
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ControllerId == controllerId, cancellationToken);
    }
}
