using Contracts.Abstractions;
using Device.Domain.Entities;

namespace Device.Domain.Interfaces;

public interface IRelayRepository : IRepository<Relay>
{
    Task<bool> ExistsAsync(
        Guid relayId, 
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Relay>> GetAllByControllerId(
        Guid controllerId,
        CancellationToken cancellationToken);
}