using Contracts.Abstractions;
using Control.Domain.Entities;

namespace Control.Domain.Interfaces;

public interface IEcosystemRepository : IRepository<EcosystemEntity>
{
    public Task<bool> ExistsAsync(
        Guid ecosystemId,
        CancellationToken cancellationToken);

    public Task<EcosystemEntity?> GetByControllerIdAsync(
        Guid controllerId,
        CancellationToken cancellationToken);
}