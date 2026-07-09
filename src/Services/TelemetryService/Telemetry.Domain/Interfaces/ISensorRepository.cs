using Contracts.Abstractions;
using Telemetry.Domain.Entities;

namespace Telemetry.Domain.Interfaces;

public interface ISensorRepository : IRepository<Sensor>
{
    public Task<bool> ExistsAsync(
        Guid sensorId,
        CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<Sensor>> GetAllByEcosystemId(
        Guid ecosystemId,
        CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<Sensor>> GetManyByIdsAsync(
        List<Guid> sensorIds,
        CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<Sensor>> GetDelayedSensors(
        DateTime offlineThreshold,
        CancellationToken cancellationToken = default);
}
