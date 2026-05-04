using Device.Domain.Interfaces;

namespace Device.Infrastructure.Extensions;

public sealed class UnitOfWork(SystemDbContext context) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }
}
