using BuildingBlocks.Domain.Abstractions;
using IdentityService.Domain.Entities;

namespace IdentityService.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    public Task<IReadOnlyList<User>> GetWithExpiredSubscriptionAsync(
        CancellationToken cancellationToken = default);

    public Task<bool> TelegramChatIdExistsAsync(
        long chatId,
        CancellationToken cancellationToken = default);

    public Task<User?> GetByTelegramChatIdAsync(
        long chatId,
        CancellationToken cancellationToken = default);
}
