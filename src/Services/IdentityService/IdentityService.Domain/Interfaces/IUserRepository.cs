using IdentityService.Domain.Entities;

namespace IdentityService.Domain.Interfaces;

public interface IUserRepository
{
    public Task<IReadOnlyList<User>> GetWithExpiredSubscriptionAsync(
        CancellationToken cancellationToken);

    public Task UpdateAsync(
        User user,
        CancellationToken cancellationToken);
}
