using Contracts.Abstractions;
using Control.Domain.Entities;

namespace Control.Domain.Interfaces;

public interface IRelayRepository : IRepository<RelayEntity>
{
    public Task<bool> ExistsAsync(
        Guid relayId,
        CancellationToken cancellationToken);

    public Task<RelayEntity?> GetByPowerSensorId(
        Guid powerSensorId,
        CancellationToken cancellationToken);

    public Task<IReadOnlyList<RelayEntity>> GetManyByIds(
        IEnumerable<Guid> relayIds,
        CancellationToken cancellationToken);
}