using BuildingBlocks.Domain.Abstractions;

namespace Device.Domain.Interfaces;

public interface IRelayCommandsRepository : IRepository<RelayCommand>
{
    public Task<int> DeleteCompletedAsync(
        CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<RelayCommand>> GetPendingByControllerIdAsync(
        Guid controllerId,
        CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<RelayCommand>> GetActiveCommandsAsync(
        CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<RelayCommand>> GetActiveByRelayIdAsync(
        Guid relayId,
        CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<RelayCommand>> GetUnexpiredPendingByControllerIdAsync(
        Guid controllerId,
        DateTime utcNow,
        CancellationToken cancellationToken = default);
}
