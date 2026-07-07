using IdentityService.Domain.Entities;

namespace IdentityService.Domain.Interfaces;

public interface ISubscriptionRepository
{
    public Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
