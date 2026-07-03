using Contracts.Abstractions;
using Control.Domain.Entities;

namespace Control.Domain.Interfaces;

public interface ISensorRepository : IRepository<Sensor>
{
    public Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken);

    public Task<IReadOnlyList<Sensor>> GetManyByIdsAsync(
        IEnumerable<Guid> sensorIds,
        CancellationToken cancellationToken);
}