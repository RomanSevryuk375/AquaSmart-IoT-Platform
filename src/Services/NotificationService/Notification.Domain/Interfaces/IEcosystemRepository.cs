using Contracts.Abstractions;
using Notification.Domain.Entities;

namespace Notification.Domain.Interfaces;

public interface IEcosystemRepository : IRepository<Ecosystem>
{
    public Task<bool> ExistsAsync(
        Guid ecosystemId,
        CancellationToken cancellationToken);

    public Task<Ecosystem?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken);
}
