using Contracts.Abstractions;
using Control.Domain.Entities;

namespace Control.Domain.Interfaces;

public interface ISensorRepository : IRepository<SensorEntity>
{
    public Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken);

    public Task<IReadOnlyList<SensorEntity>> GetManyByIdsAsync(
        IEnumerable<Guid> sensorIds,
        CancellationToken cancellationToken);
}