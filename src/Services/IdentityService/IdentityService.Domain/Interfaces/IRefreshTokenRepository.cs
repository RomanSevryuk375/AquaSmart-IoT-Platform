using IdentityService.Domain.Entities;

namespace IdentityService.Domain.Interfaces;

public interface IRefreshTokenRepository
{
    Task<string> AddTokenAsync(
        RefreshTokenEntity refreshToken, 
        CancellationToken cancellationToken);

    Task DeleteIncorrectTokensAsync(
        CancellationToken cancellationToken);

    Task<RefreshTokenEntity?> GetByIdAsync(
        Guid tokenId, 
        CancellationToken cancellationToken);

    Task DeleteTokensByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task UpdateTokenAsync(
        RefreshTokenEntity token, 
        CancellationToken cancellationToken);
}