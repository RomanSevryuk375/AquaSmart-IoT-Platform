using Contracts.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Control.Infrastructure.Persistence.Repositories;

public abstract class BaseRepository<T>(ControlDbContext dbContext)
    : IRepository<T> where T : class, IEntity
{
    private readonly DbSet<T> _set = dbContext.Set<T>();
    protected ControlDbContext Context => dbContext;

    public async Task<Guid> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _set.AddAsync(entity, cancellationToken);

        return entity.Id;
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _set.FindAsync([id], cancellationToken: cancellationToken);

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _set.Where(x => x.Id == id).ExecuteDeleteAsync(cancellationToken);
}
