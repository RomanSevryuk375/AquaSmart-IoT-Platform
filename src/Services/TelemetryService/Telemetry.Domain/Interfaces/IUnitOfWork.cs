namespace Telemetry.Domain.Interfaces;

public interface IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken = default);

    public Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    public Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
