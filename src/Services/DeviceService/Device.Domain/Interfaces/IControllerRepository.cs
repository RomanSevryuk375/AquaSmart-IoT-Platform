namespace Device.Domain.Interfaces;

public interface IControllerRepository : IRepository<Controller>
{
    public Task<Controller?> GetByMacAddressAsync(
        string macAddress,
        CancellationToken cancellationToken = default);

    public Task<Controller?> GetByDeviceTokenAsync(
        string deviceTokenHash,
        CancellationToken cancellationToken = default);
}
