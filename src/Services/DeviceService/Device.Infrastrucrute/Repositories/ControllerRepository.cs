using Device.Domain.Entities;
using Device.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Device.Infrastructure.Repositories;

public sealed class ControllerRepository(SystemDbContext dbContext) 
    : BaseRepository<ControllerEntity>(dbContext), IControllerRepository
{
    public async Task<ControllerEntity?> GetByMacAddressAsync(
        string macAddress, 
        CancellationToken cancellationToken)
    {
        return await Context.Controllers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MacAddress == macAddress, cancellationToken);
    }

    public async Task<ControllerEntity?> GetByDeviceTokenAsync(
        string deviceTokenHash, 
        CancellationToken cancellationToken)
    {
        return await Context.Controllers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.DeviceTokenHash == deviceTokenHash, cancellationToken);
    }
}
