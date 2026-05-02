using Microsoft.EntityFrameworkCore;
using Telemetry.Domain.Entities;
using Telemetry.Domain.Interfaces;

namespace Telemetry.Infrastructure.Repositories;

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

    public async Task<IReadOnlyList<SensorEntity>> GetAllByEcosystemId(
        Guid ecosystemId,
        CancellationToken cancellationToken)
    {
        return await Context.Sensors
            .AsNoTracking()
            .Where(x => x.EcosystemId == ecosystemId)
            .ToListAsync(cancellationToken);
    }
}
