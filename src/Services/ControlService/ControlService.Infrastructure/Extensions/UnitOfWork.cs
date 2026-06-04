using Control.Domain.Interfaces;
using Control.Infrastructure.Persistence;

namespace Control.Infrastructure.Extensions;

public sealed class UnitOfWork(SystemDbContext dbContext) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
