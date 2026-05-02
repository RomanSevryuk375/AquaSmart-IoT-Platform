using Contracts.Abstractions;
using Device.Domain.Entities;

namespace Device.Domain.Interfaces;

public interface IControllerRepository : IRepository<ControllerEntity>
{
    Task<ControllerEntity?> GetByMacAddressAsync(
        string macAddress, 
        CancellationToken cancellationToken);

    Task<ControllerEntity?> GetByDeviceTokenAsync(
        string deviceTokenHash,
        CancellationToken cancellationToken);
}
