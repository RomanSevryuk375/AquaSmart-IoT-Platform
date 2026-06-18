using Contracts.Abstractions;
using Device.Domain.Entities;

namespace Device.Domain.Interfaces;

public interface IRelayCommandsQueueRepository : IRepository<RelayCommand>
{
    Task DeleteCompletedAsync(
        CancellationToken cancellationToken);
    Task<IReadOnlyList<RelayCommand>> GetPendingByControllerIdAsync(
        Guid controllerId, 
        CancellationToken cancellationToken);
}