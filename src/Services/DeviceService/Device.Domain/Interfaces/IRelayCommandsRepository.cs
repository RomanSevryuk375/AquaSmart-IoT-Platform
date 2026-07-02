namespace Device.Domain.Interfaces;

public interface IRelayCommandsRepository : IRepository<RelayCommand>
{
    public Task<int> DeleteCompletedAsync(
        CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<RelayCommand>> GetPendingByControllerIdAsync(
        Guid controllerId,
        CancellationToken cancellationToken = default);
}
