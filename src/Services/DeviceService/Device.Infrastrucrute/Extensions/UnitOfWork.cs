using Device.Domain.Interfaces;
using Device.Infrastructure.Persistence;

namespace Device.Infrastructure.Extensions;

public sealed class UnitOfWork(DeviceDbContext context) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }
}
