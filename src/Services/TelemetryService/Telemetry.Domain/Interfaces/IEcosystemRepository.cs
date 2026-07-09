using Contracts.Abstractions;
using Telemetry.Domain.Entities;

namespace Telemetry.Domain.Interfaces;

public interface IEcosystemRepository : IRepository<Ecosystem>
{
    public Task<Ecosystem?> GetByControllerIdAsync(
        Guid controllerId,
        CancellationToken cancellationToken = default);
}
