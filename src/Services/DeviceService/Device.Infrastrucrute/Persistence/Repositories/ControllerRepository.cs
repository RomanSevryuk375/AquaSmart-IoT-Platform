using BuildingBlocks.Domain.Results;
using BuildingBlocks.Infrastructure.Data;
using Device.Domain.ValueObjects;

namespace Device.Infrastructure.Persistence.Repositories;

public sealed class ControllerRepository(DeviceDbContext dbContext)
    : BaseRepository<DeviceDbContext, Controller>(dbContext), IControllerRepository
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

    public async Task<IReadOnlyList<Controller>> GetOfflineControllersAsync(
        DateTime offlineThreshold,
        CancellationToken cancellationToken = default)
    {
        return await Context.Controllers
            .Where(x => x.IsOnline && x.LastSeenAt < offlineThreshold)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Controller>> GetOfflinePendingAlertAsync(
        DateTime graceThreshold,
        CancellationToken cancellationToken = default)
    {
        return await Context.Controllers
            .Where(x => !x.IsOnline && !x.OfflineAlertSent && x.LastSeenAt < graceThreshold)
            .ToListAsync(cancellationToken);
    }
}
