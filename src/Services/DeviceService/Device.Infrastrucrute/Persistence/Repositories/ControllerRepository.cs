namespace Device.Infrastructure.Persistence.Repositories;

public sealed class ControllerRepository(SystemDbContext dbContext) 
    : BaseRepository<Controller>(dbContext), IControllerRepository
{
    public async Task<Controller?> GetByMacAddressAsync(
        string macAddress, 
        CancellationToken cancellationToken = default)
    {
        return await Context.Controllers
            .FirstOrDefaultAsync(x => x.MacAddress.Value == macAddress, cancellationToken);
    }

    public async Task<Controller?> GetByDeviceTokenAsync(
        string deviceTokenHash, 
        CancellationToken cancellationToken = default)
    {
        return await Context.Controllers
            .FirstOrDefaultAsync(x => x.DeviceTokenHash == deviceTokenHash, cancellationToken);
    }
}
