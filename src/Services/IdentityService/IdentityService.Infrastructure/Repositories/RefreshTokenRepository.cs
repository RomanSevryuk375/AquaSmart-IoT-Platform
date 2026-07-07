using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Repositories;

public sealed class RefreshTokenRepository(IdentityDbContext dbContext)
    : BaseRepository<RefreshToken>(dbContext), IRefreshTokenRepository
{
    public async Task DeleteTokensByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await Context.RefreshTokens
            .Where(x => x.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task DeleteIncorrectTokensAsync(
        CancellationToken cancellationToken = default)
    {
        await Context.RefreshTokens
            .Where(x => x.IsUsed
                     || x.IsRevoked
                     || (x.ExpiredAt < DateTime.UtcNow))
            .ExecuteDeleteAsync(cancellationToken);
    }
}
