using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Repositories;

public class UserRepository(IdentityDbContext dbContext)
    : IUserRepository
{
    public async Task<IReadOnlyList<User>> GetWithExpiredSubscriptionAsync(
        CancellationToken cancellationToken)
    {
        return await dbContext.Users
            .AsNoTracking()
            .Where(x => (x.SubscriptionEndDate < DateTime.UtcNow)
                     && (x.SubscriptionId != Guid.Parse(Contracts.Enums.Subscription.Free)))
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        User user,
        CancellationToken cancellationToken)
    {
        User trackedEntity = await dbContext.Users.FindAsync(user.Id, cancellationToken)
           ?? throw new KeyNotFoundException($"{nameof(User)} with id {user.Id} not found.");

        dbContext.Entry(trackedEntity).CurrentValues.SetValues(user);
    }
}
