using Contracts.Abstractions;
using Control.Domain.Entities;

namespace Control.Domain.Interfaces;

public interface ISensorRepository : IRepository<SensorEntity>
{
    Task<bool> ExistsAsync(
        Guid id, 
        CancellationToken cancellationToken);

    Task<IReadOnlyList<SensorEntity>> GetManyByIdsAsync(
        IEnumerable<Guid> sensorIds,
        CancellationToken cancellationToken);
}