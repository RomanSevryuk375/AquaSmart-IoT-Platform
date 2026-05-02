using Contracts.Enums;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Repositories;

public class UserRepository(IdentityDbContext dbContext) 
    : IUserRepository
{
    public async Task<IReadOnlyList<UserEntity>> GetWithExpiredSubscriptionAsync(
        CancellationToken cancellationToken)
    {
        return await dbContext.Users
            .AsNoTracking()
            .Where(x => (x.SubscriptionEndDate < DateTime.UtcNow) 
                     && (x.SubscriptionId != Guid.Parse(SubscriptionEnum.Free)))
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        UserEntity user,
        CancellationToken cancellationToken)
    {
        var trackedEntity = await dbContext.Users.FindAsync(user.Id, cancellationToken)
           ?? throw new KeyNotFoundException($"{nameof(UserEntity)} with id {user.Id} not found.");

        dbContext.Entry(trackedEntity).CurrentValues.SetValues(user);
    }
}
