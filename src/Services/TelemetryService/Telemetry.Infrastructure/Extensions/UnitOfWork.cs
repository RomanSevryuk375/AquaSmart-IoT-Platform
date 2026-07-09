using Telemetry.Domain.Interfaces;
using Telemetry.Infrastructure.Persistence;

namespace Telemetry.Infrastructure.Extensions;

public class UnitOfWork(SystemDbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => context.SaveChangesAsync(cancellationToken);
}
