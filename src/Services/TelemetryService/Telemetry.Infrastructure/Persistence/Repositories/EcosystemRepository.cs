using Microsoft.EntityFrameworkCore;
using Telemetry.Domain.Entities;
using Telemetry.Domain.Interfaces;

namespace Telemetry.Infrastructure.Persistence.Repositories;

public sealed class EcosystemRepository(TelemetryDbContext dbContext)
    : BaseRepository<Ecosystem>(dbContext), IEcosystemRepository
{
    public async Task<Ecosystem?> GetByControllerIdAsync(
        Guid controllerId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Ecosystems
            .FirstOrDefaultAsync(x => x.ControllerId == controllerId, cancellationToken);
    }
}
