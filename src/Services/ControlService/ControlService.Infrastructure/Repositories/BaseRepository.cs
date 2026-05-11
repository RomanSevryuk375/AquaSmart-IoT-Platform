using Contracts.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Control.Infrastructure.Repositories;

public abstract class BaseRepository<T>(SystemDbContext dbContext) : IRepository<T> where T : class, IEntity
{
    private readonly DbSet<T> _set = dbContext.Set<T>();
    protected SystemDbContext Context => dbContext;

    public async Task<Guid> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _set.AddAsync(entity, cancellationToken);

        return entity.Id;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _set.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return;
        }

        _set.Remove(entity);
    }

    public async Task<IReadOnlyList<T>> GetAllAsync(
        BaseSpecification<T>? specification,
        int? skip,
        int? take,
        CancellationToken cancellationToken = default)
    {
        if (skip is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(skip), "Skip cannot be negative.");
        }

        if (take is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(take), "Take cannot be negative.");
        }

        var query = _set.AsNoTracking().AsQueryable();

        if (specification is not null)
        {
            query = query.Where(specification.Criteria);
        }

        query = query.OrderBy(x => x.Id);

        if (skip.HasValue)
        {
            query = query.Skip(skip.Value);
        }

        if (take.HasValue)
        {
            query = query.Take(take.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _set.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken)
    {
        var trackedEntity = await _set.FindAsync([entity.Id], cancellationToken)
            ?? throw new KeyNotFoundException($"{nameof(T)} with id {entity.Id} not found.");

        dbContext.Entry(trackedEntity).CurrentValues.SetValues(entity);
    }
}
