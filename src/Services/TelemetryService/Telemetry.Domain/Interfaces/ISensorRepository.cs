using Contracts.Abstractions;
using Telemetry.Domain.Entities;

namespace Telemetry.Domain.Interfaces;

public interface ISensorRepository : IRepository<SensorEntity>
{
    Task<bool> ExistsAsync(
        Guid sensorId, 
        CancellationToken cancellationToken);

    Task<IReadOnlyList<SensorEntity>> GetAllByEcosystemId(
        Guid ecosystemId,
        CancellationToken cancellationToken);
}
