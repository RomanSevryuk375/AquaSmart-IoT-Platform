using IdentityService.Domain.Entities;

namespace IdentityService.Domain.Interfaces;

public interface ISubscriptionRepository
{
    Task<SubscriptionEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}