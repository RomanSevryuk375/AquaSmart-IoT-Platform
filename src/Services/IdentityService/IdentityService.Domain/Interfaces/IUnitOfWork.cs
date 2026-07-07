namespace IdentityService.Domain.Interfaces;

public interface IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken);
}
