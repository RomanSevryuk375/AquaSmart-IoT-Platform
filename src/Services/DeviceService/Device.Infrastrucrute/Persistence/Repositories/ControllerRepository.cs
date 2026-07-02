using Contracts.Results;
using Device.Domain.ValueObjects;

namespace Device.Infrastructure.Persistence.Repositories;

public sealed class ControllerRepository(SystemDbContext dbContext)
    : BaseRepository<Controller>(dbContext), IControllerRepository
{
    public async Task<Controller?> GetByMacAddressAsync(
        string macAddress,
        CancellationToken cancellationToken = default)
    {
        Result<MacAddress> macResult = MacAddress.Create(macAddress);
        if (macResult.IsFailure)
        {
            return null;
        }

        return await Context.Controllers
            .FirstOrDefaultAsync(x => x.MacAddress == macResult.Value, cancellationToken);
    }

    public async Task<Controller?> GetByDeviceTokenAsync(
        string deviceTokenHash,
        CancellationToken cancellationToken = default)
    {
        return await Context.Controllers
            .FirstOrDefaultAsync(x => x.DeviceTokenHash == deviceTokenHash, cancellationToken);
    }
}
