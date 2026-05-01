using Contracts.Abstractions;
using Notification.Domain.Entities;

namespace Notification.Domain.Interfaces;

public interface IEcosystemRepository : IRepository<EcosystemEntity>
{
    Task<bool> ExistsAsync(
        Guid ecosystemId, 
        CancellationToken cancellationToken);

    Task<EcosystemEntity?> GetByUserIdAsync(
        Guid userId, 
        CancellationToken cancellationToken);
}
