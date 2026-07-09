namespace Contracts.Abstractions;

public interface IRepository<T> where T : class, IEntity
{
    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    public Task<Guid> AddAsync(T entity, CancellationToken cancellationToken = default);
    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
