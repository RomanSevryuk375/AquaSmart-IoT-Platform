using Contracts.Abstractions;
using Telemetry.Domain.Entities;

namespace Telemetry.Domain.Interfaces;

public interface IEcosystemRepository : IRepository<EcosystemEntity>
{
    Task<EcosystemEntity?> GetByControllerIdAsync(
        Guid controllerId,
        CancellationToken cancellationToken);
}