using IdentityService.Domain.Entities;

namespace IdentityService.Domain.Interfaces;

public interface IUserRepository
{
    Task<IReadOnlyList<UserEntity>> GetWithExpiredSubscriptionAsync(
        CancellationToken cancellationToken);

    Task UpdateAsync(
        UserEntity user, 
        CancellationToken cancellationToken);
}