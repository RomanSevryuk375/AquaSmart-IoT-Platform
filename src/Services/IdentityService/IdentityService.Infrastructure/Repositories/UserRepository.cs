using Contracts.Enums;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Repositories;

public sealed class UserRepository(IdentityDbContext dbContext)
    : BaseRepository<User>(dbContext), IUserRepository
{
    public async Task<IReadOnlyList<User>> GetWithExpiredSubscriptionAsync(
        CancellationToken cancellationToken = default)
    {
        return await Context.Users
            .Where(x => (x.SubscriptionEndDate < DateTime.UtcNow)
                     && (x.SubscriptionId != Guid.Parse(SubscriptionType.Free)))
            .ToListAsync(cancellationToken);
    }
}
