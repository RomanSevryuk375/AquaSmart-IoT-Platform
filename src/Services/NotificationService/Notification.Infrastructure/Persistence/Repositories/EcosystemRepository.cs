using Microsoft.EntityFrameworkCore;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;

namespace Notification.Infrastructure.Persistence.Repositories;

public sealed class EcosystemRepository(NotificationDbContext dbContext)
    : BaseRepository<Ecosystem>(dbContext), IEcosystemRepository
{
    public async Task<bool> ExistsAsync
        (Guid ecosystemId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Aquariums
            .AsNoTracking()
            .AnyAsync(x => x.Id == ecosystemId, cancellationToken);
    }

    public async Task<Ecosystem?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Aquariums
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }
}
