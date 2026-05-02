using Contracts.Abstractions;
using Device.Domain.Entities;

namespace Device.Domain.Interfaces;

public interface IRelayRepository : IRepository<RelayEntity>
{
    Task<bool> ExistsAsync(
        Guid relayId, 
        CancellationToken cancellationToken);

    Task<IReadOnlyList<RelayEntity>> GetAllByControllerId(
        Guid controllerId,
        CancellationToken cancellationToken);
}