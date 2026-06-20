namespace Device.Domain.Interfaces;

public interface IRelayCommandsRepository : IRepository<RelayCommand>
{
    public Task DeleteCompletedAsync(
        CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<RelayCommand>> GetPendingByControllerIdAsync(
        Guid controllerId, 
        CancellationToken cancellationToken = default);
}
