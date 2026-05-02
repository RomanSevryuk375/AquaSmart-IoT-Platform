using Contracts.Abstractions;
using Device.Domain.Entities;

namespace Device.Domain.Interfaces;

public interface IRelayCommandsQueueRepository : IRepository<RelayCommandsQueueEntity>
{
    Task DeleteCompletedAsync(
        CancellationToken cancellationToken);
    Task<IReadOnlyList<RelayCommandsQueueEntity>> GetPendingByControllerIdAsync(
        Guid controllerId, 
        CancellationToken cancellationToken);
}