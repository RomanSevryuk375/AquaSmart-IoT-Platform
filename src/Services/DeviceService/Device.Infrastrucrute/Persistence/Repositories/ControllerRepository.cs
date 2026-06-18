using Device.Domain.Entities;
using Device.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Device.Infrastructure.Persistence.Repositories;

public sealed class ControllerRepository(SystemDbContext dbContext) 
    : BaseRepository<Controller>(dbContext), IControllerRepository
{
    public async Task<Controller?> GetByMacAddressAsync(
        string macAddress, 
        CancellationToken cancellationToken)
    {
        return await Context.Controllers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MacAddress == macAddress, cancellationToken);
    }

    public async Task<Controller?> GetByDeviceTokenAsync(
        string deviceTokenHash, 
        CancellationToken cancellationToken)
    {
        return await Context.Controllers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.DeviceTokenHash == deviceTokenHash, cancellationToken);
    }
}
