using Contracts.Abstractions;
using Control.Domain.Entities;

namespace Control.Domain.Interfaces;

public interface IAquariumRepository : IRepository<EcosystemEntity>
{
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
    Task<EcosystemEntity?> GetByControllerIdAsync(Guid controllerId, CancellationToken cancellationToken);
}