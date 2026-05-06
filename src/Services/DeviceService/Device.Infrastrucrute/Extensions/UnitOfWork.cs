using Device.Domain.Interfaces;

namespace Device.Infrastructure.Extensions;

public sealed class UnitOfWork(DeviceDbContext context) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }
}
