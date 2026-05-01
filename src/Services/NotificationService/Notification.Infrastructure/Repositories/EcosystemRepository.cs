using Microsoft.EntityFrameworkCore;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;

namespace Notification.Infrastructure.Repositories;

public sealed class EcosystemRepository(SystemDbContext dbContext)
    : BaseRepository<EcosystemEntity>(dbContext), IEcosystemRepository
{
    public async Task<bool> ExistsAsync
        (Guid ecosystemId,
        CancellationToken cancellationToken)
    {
        return await Context.Aquariums
            .AsNoTracking()
            .AnyAsync(x => x.Id == ecosystemId, cancellationToken);
    }

    public async Task<EcosystemEntity?> GetByUserIdAsync(
        Guid userId, 
        CancellationToken cancellationToken)
    {
        return await Context.Aquariums
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }
}
