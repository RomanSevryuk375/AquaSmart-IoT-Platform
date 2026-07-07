using IdentityService.Domain.Entities;

namespace IdentityService.Domain.Interfaces;

public interface IRefreshTokenRepository
{
    public Task<string> AddTokenAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken);

    public Task DeleteIncorrectTokensAsync(
        CancellationToken cancellationToken);

    public Task<RefreshToken?> GetByIdAsync(
        Guid tokenId,
        CancellationToken cancellationToken);

    public Task DeleteTokensByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken);

    public Task UpdateTokenAsync(
        RefreshToken token,
        CancellationToken cancellationToken);
}
