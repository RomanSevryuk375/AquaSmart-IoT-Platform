using Notification.Domain.Interfaces;

namespace Notification.Infrastructure.Extensions;

public class UnitOfWork(SystemDbContext dbContext) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
