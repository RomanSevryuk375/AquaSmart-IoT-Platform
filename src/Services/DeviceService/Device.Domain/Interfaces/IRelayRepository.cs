namespace Device.Domain.Interfaces;

public interface IRelayRepository : IRepository<Relay>
{
    public Task<bool> ExistsAsync(
        Guid relayId, 
        CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<Relay>> GetAllByControllerId(
        Guid controllerId,
        CancellationToken cancellationToken = default);
}
