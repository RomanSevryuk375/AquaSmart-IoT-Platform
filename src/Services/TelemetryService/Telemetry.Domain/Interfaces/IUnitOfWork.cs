namespace Telemetry.Domain.Interfaces;

public interface IUnitOfWork
{
    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
