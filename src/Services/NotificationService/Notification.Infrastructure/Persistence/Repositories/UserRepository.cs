using BuildingBlocks.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;

namespace Notification.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(NotificationDbContext dbContext)
    : BaseRepository<NotificationDbContext, User>(dbContext), IUserRepository
{
    public async Task<bool> ExistsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Users
            .AsNoTracking()
            .AnyAsync(x => x.Id == userId, cancellationToken);
    }

    public async Task<List<User>> GetAllUsersByIdAsync(
        List<Guid> userIds,
        CancellationToken cancellationToken = default)
    {
        return await Context.Users
            .AsNoTracking()
            .Where(x => userIds.Contains(x.Id))
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

}
