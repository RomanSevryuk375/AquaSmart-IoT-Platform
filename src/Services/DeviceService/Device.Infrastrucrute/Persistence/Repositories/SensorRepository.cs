namespace Device.Infrastructure.Persistence.Repositories;

public sealed class SensorRepository(SystemDbContext dbContext)
    : BaseRepository<Sensor>(dbContext), ISensorRepository
{
    public async Task<bool> ExistsAsync(
        Guid sensorId, 
        CancellationToken cancellationToken = default)
    {
        return await Context.Sensors
            .AnyAsync(x => x.Id == sensorId, cancellationToken);
    }

    public async Task<IReadOnlyList<Sensor>> GetAllSensorsAsync(
        Guid controllerId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Sensors
            .Where(x => x.ControllerId == controllerId)
            .ToListAsync(cancellationToken);
    }
}
