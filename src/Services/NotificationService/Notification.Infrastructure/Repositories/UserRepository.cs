using Microsoft.EntityFrameworkCore;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;

namespace Notification.Infrastructure.Repositories;

public sealed class UserRepository(SystemDbContext dbContext)
    : BaseRepository<UserEntity>(dbContext), IUserRepository
{
    public async Task<bool> ExistsAsync(
        Guid userId, 
        CancellationToken cancellationToken)
    {
        return await Context.Users
            .AsNoTracking()
            .AnyAsync(x => x.Id == userId, cancellationToken);
    }

    public async Task<List<UserEntity>> GetAllUsersByIdAsync(
        List<Guid> userIds,
        CancellationToken cancellationToken)
    {
        return await Context.Users
            .AsNoTracking()
            .Where(x => userIds.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }
}
