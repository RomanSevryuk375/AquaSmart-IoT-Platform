using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Infrastructure.Data;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(IdentityDbContext dbContext)
    : BaseRepository<IdentityDbContext, User>(dbContext), IUserRepository
{
    public async Task<IReadOnlyList<User>> GetWithExpiredSubscriptionAsync(
        CancellationToken cancellationToken = default)
    {
        return await Context.Users
            .Where(x => (x.SubscriptionEndDate < DateTime.UtcNow)
                     && (x.SubscriptionId != Guid.Parse(SubscriptionType.Free)))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> TelegramChatIdExistsAsync(
        long chatId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Users
            .AsNoTracking()
            .AnyAsync(x => x.TelegramChatId == chatId, cancellationToken);
    }

    public async Task<User?> GetByTelegramChatIdAsync(
        long chatId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Users
            .FirstOrDefaultAsync(x => x.TelegramChatId == chatId, cancellationToken);
    }
}
