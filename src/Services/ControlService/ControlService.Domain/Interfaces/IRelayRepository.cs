using Contracts.Abstractions;
using Control.Domain.Entities;

namespace Control.Domain.Interfaces;

public interface IRelayRepository : IRepository<RelayEntity>
{
    Task<bool> ExistsAsync(
        Guid relayId, 
        CancellationToken cancellationToken);

    Task<RelayEntity?> GetByPowerSensorId(
        Guid powerSensorId,
        CancellationToken cancellationToken);
}