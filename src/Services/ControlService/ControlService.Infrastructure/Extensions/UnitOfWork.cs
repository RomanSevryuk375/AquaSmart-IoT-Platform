using Control.Domain.Interfaces;

namespace Control.Infrastructure.Extensions;

public class UnitOfWork(ControlDbContext dbContext) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
