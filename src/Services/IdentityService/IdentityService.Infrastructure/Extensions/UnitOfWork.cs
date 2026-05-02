using IdentityService.Domain.Interfaces;

namespace IdentityService.Infrastructure.Extensions;

public class UnitOfWork(IdentityDbContext dbContext) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
