namespace Device.Infrastructure.Persistence.Repositories;

public sealed class SensorRepository(SystemDbContext dbContext)
    : BaseRepository<Sensor>(dbContext), ISensorRepository
{
    public async Task<bool> ExistsAsync(
        Guid sensorId, 
        CancellationToken cancellationToken)
    {
        return await Context.Sensors
            .AsNoTracking()
            .AnyAsync(x => x.Id == sensorId, cancellationToken);
    }

    public async Task<IReadOnlyList<Sensor>> GetAllSensorsAsync(
        Guid controllerId,
        CancellationToken cancellationToken)
    {
        return await Context.Sensors
            .AsNoTracking()
            .Where(x => x.ControllerId == controllerId)
            .ToListAsync(cancellationToken);
    }
}
