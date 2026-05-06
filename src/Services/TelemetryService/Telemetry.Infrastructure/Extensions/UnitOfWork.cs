using Telemetry.Domain.Interfaces;

namespace Telemetry.Infrastructure.Extensions;

public class UnitOfWork(TelemetryDbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return context.SaveChangesAsync(cancellationToken);
    }
}
