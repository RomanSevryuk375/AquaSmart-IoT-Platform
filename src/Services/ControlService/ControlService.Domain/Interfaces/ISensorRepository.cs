using Contracts.Abstractions;
using Control.Domain.Entities;

namespace Control.Domain.Interfaces;

public interface ISensorRepository : IRepository<Sensor>
{
    public Task<bool> ExistsAsync(
        Guid sensorId,
        CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<Sensor>> GetManyByIdsAsync(
        IEnumerable<Guid> sensorIds,
        CancellationToken cancellationToken = default);
}
