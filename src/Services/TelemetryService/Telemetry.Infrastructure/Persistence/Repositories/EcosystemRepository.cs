using Microsoft.EntityFrameworkCore;
using Telemetry.Domain.Entities;
using Telemetry.Domain.Interfaces;

namespace Telemetry.Infrastructure.Persistence.Repositories;

public sealed class EcosystemRepository(TelemetryDbContext dbContext) 
    : BaseRepository<EcosystemEntity>(dbContext), IEcosystemRepository
{
    public async Task<EcosystemEntity?> GetByControllerIdAsync(
        Guid controllerId,
        CancellationToken cancellationToken)
    {
        return await Context.Ecosystems
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ControllerId == controllerId, cancellationToken);
    }
}
