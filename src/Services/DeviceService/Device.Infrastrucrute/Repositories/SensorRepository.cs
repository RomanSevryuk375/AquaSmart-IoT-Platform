using Device.Domain.Entities;
using Device.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Device.Infrastructure.Repositories;

public sealed class SensorRepository(SystemDbContext dbContext)
    : BaseRepository<SensorEntity>(dbContext), ISensorRepository
{
    public async Task<bool> ExistsAsync(
        Guid sensorId, 
        CancellationToken cancellationToken)
    {
        return await Context.Sensors
            .AsNoTracking()
            .AnyAsync(x => x.Id == sensorId, cancellationToken);
    }

    public async Task<IReadOnlyList<SensorEntity>> GetAllSensorsAsync(
        Guid controllerId,
        CancellationToken cancellationToken)
    {
        return await Context.Sensors
            .AsNoTracking()
            .Where(x => x.ControllerId == controllerId)
            .ToListAsync(cancellationToken);
    }
}
