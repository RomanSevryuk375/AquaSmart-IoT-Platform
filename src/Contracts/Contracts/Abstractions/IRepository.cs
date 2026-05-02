namespace Contracts.Abstractions;

public interface IRepository<T> where T : class, IEntity
{
    Task<IReadOnlyList<T>> GetAllAsync(
        BaseSpecification<T>? specification,
        int? skip,
        int? take,
        CancellationToken cancellationToken = default);

    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Guid> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}