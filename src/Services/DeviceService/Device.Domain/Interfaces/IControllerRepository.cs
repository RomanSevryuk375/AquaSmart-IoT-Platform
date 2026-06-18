using Contracts.Abstractions;
using Device.Domain.Entities;

namespace Device.Domain.Interfaces;

public interface IControllerRepository : IRepository<Controller>
{
    Task<Controller?> GetByMacAddressAsync(
        string macAddress, 
        CancellationToken cancellationToken);

    Task<Controller?> GetByDeviceTokenAsync(
        string deviceTokenHash,
        CancellationToken cancellationToken);
}
