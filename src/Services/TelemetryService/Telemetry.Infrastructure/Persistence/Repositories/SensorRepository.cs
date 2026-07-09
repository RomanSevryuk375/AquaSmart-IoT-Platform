using Contracts.Enums;
using Microsoft.EntityFrameworkCore;
using Telemetry.Domain.Entities;
using Telemetry.Domain.Interfaces;

namespace Telemetry.Infrastructure.Persistence.Repositories;

public sealed class SensorRepository(TelemetryDbContext dbContext)
    : BaseRepository<Sensor>(dbContext), ISensorRepository
{
    public async Task<bool> ExistsAsync(
        Guid sensorId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Sensors
            .AnyAsync(x => x.Id == sensorId, cancellationToken);
    }

    public async Task<IReadOnlyList<Sensor>> GetAllByEcosystemId(
        Guid ecosystemId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Sensors
            .Where(x => x.EcosystemId == ecosystemId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Sensor>> GetDelayedSensors(
        DateTime offlineThreshold,
        CancellationToken cancellationToken = default)
    {
        return await Context.Sensors
            .Where(x => x.UpdatedAt < offlineThreshold &&
                        x.State == SensorState.Active)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Sensor>> GetManyByIdsAsync(
        List<Guid> sensorIds,
        CancellationToken cancellationToken = default)
    {
        return await Context.Sensors
            .Where(x => sensorIds.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }
}
