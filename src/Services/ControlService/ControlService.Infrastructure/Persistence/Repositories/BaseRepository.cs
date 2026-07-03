using Contracts.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Control.Infrastructure.Persistence.Repositories;

public abstract class BaseRepository<T>(SystemDbContext dbContext)
    : IRepository<T> where T : class, IEntity
{
    private readonly DbSet<T> _set = dbContext.Set<T>();
    protected SystemDbContext Context => dbContext;

    public async Task<Guid> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _set.AddAsync(entity, cancellationToken);

        return entity.Id;
    }

    public async Task<IReadOnlyList<T>> GetAllAsync(
        BaseSpecification<T>? specification,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _set.AsQueryable();

        if (specification is not null)
        {
            query = query.Where(specification.Criteria);

            query = specification.Includes.Aggregate(
                query, (current, include) => current.Include(include));
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _set.FindAsync([id], cancellationToken: cancellationToken);

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _set.Where(x => x.Id == id).ExecuteDeleteAsync(cancellationToken);
}
