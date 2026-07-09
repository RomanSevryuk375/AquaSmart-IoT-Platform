using Contracts.Abstractions;
using Control.Domain.Entities;

namespace Control.Domain.Interfaces;

public interface IRelayRepository : IRepository<Relay>
{
    public Task<bool> ExistsAsync(
        Guid relayId,
        CancellationToken cancellationToken = default);

    public Task<Relay?> GetByPowerSensorId(
        Guid powerSensorId,
        CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<Relay>> GetManyByIds(
        IEnumerable<Guid> relayIds,
        CancellationToken cancellationToken = default);
}
