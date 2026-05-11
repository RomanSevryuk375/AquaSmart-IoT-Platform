using Telemetry.Domain.Interfaces;
using Telemetry.Infrastructure.Persistence;

namespace Telemetry.Infrastructure.Extensions;

public class UnitOfWork(TelemetryDbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return context.SaveChangesAsync(cancellationToken);
    }
}
