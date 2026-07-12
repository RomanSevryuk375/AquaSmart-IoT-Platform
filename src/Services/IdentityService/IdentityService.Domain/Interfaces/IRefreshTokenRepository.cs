using Contracts.Abstractions;
using IdentityService.Domain.Entities;

namespace IdentityService.Domain.Interfaces;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    public Task DeleteIncorrectTokensAsync(
        CancellationToken cancellationToken = default);

    public Task DeleteTokensByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
