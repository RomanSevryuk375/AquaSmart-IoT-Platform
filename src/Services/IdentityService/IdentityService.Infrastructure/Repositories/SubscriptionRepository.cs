using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Repositories;

public class SubscriptionRepository(
    IdentityDbContext dbContext) : ISubscriptionRepository
{
    public async Task<SubscriptionEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Subscriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }
}
