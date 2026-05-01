using Contracts.Abstractions;
using Notification.Domain.Entities;

namespace Notification.Domain.Interfaces;

public interface IAquariumRepository : IRepository<EcosystemEntity>
{
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
    Task<EcosystemEntity?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
