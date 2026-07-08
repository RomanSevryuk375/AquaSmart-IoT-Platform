using Microsoft.EntityFrameworkCore.Storage;
using Notification.Domain.Interfaces;
using Notification.Infrastructure.Persistence;

namespace Notification.Infrastructure.Extensions;

public sealed class UnitOfWork(NotificationDbContext dbContext) : IUnitOfWork
{
    private IDbContextTransaction? _contextTransaction;
    public async Task BeginTransactionAsync(CancellationToken cancellationToken)
    {
        _contextTransaction =
            await dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            if (_contextTransaction is not null)
            {
                await _contextTransaction.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_contextTransaction is not null)
            {
                await _contextTransaction.DisposeAsync();
                _contextTransaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken)
    {
        if (_contextTransaction is not null)
        {
            await _contextTransaction.RollbackAsync(cancellationToken);
            await _contextTransaction.DisposeAsync();
            _contextTransaction = null;
        }
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken) =>
        await dbContext.SaveChangesAsync(cancellationToken);
}
