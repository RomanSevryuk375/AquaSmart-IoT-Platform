using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Repositories;

public class RefreshTokenRepository(IdentityDbContext dbContext) 
    : IRefreshTokenRepository
{
    public async Task<string> AddTokenAsync(
        RefreshTokenEntity refreshToken,
        CancellationToken cancellationToken)
    {
        await dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);

        return refreshToken.TokenHash;
    }

    public async Task<RefreshTokenEntity?> GetByIdAsync(
        Guid tokenId,
        CancellationToken cancellationToken)
    {
        return await dbContext.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == tokenId, cancellationToken);
    }

    public async Task DeleteTokensByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        await dbContext.RefreshTokens
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task DeleteIncorrectTokensAsync(
        CancellationToken cancellationToken)
    {
        await dbContext.RefreshTokens
            .AsNoTracking()
            .Where(x =>
                (x.IsUsed == true)
                || (x.IsRevoked == true)
                || (x.ExpiredAt < DateTime.UtcNow))
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task UpdateTokenAsync(
        RefreshTokenEntity token,
        CancellationToken cancellationToken)
    {
        var trackedEntity = await dbContext.RefreshTokens.FindAsync(token.Id, cancellationToken)
           ?? throw new KeyNotFoundException($"{nameof(RefreshTokenEntity)} with id {token.Id} not found.");

        dbContext.Entry(trackedEntity).CurrentValues.SetValues(token);
    }
}
