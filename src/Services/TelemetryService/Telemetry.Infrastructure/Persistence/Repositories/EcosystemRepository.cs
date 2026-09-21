using BuildingBlocks.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Telemetry.Domain.Entities;
using Telemetry.Domain.Interfaces;

namespace Telemetry.Infrastructure.Persistence.Repositories;

public sealed class EcosystemRepository(TelemetryDbContext dbContext)
    : BaseRepository<TelemetryDbContext, Ecosystem>(dbContext), IEcosystemRepository
{
    public async Task<Ecosystem?> GetByControllerIdAsync(
        Guid controllerId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Ecosystems
            .FirstOrDefaultAsync(x => x.ControllerId == controllerId, cancellationToken);
    }

    public async Task<bool> UserOwnsEcosystemAsync(
        Guid ecosystemId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Ecosystems
            .AsNoTracking()
            .AnyAsync(x => x.Id == ecosystemId && x.UserId == userId, cancellationToken);
    }
}
