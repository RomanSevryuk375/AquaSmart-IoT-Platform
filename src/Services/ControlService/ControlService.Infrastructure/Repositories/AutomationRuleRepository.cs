using Contracts.Abstractions;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Control.Infrastructure.Repositories;

public sealed class AutomationRuleRepository(ControlDbContext dbContext)
        : BaseRepository<AutomationRuleEntity>(dbContext), IAutomationRuleRepository
{
    public async Task<AutomationRuleEntity?> GetByIdWithConditionsAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await Context.Rules
            .Include(x => x.Conditions) 
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<AutomationRuleEntity>> GetBySensorIdWithConditionsAsync(
        Guid sensorId,
        CancellationToken cancellationToken)
    {
        return await Context.Rules
            .Include(x => x.Conditions)
            .Where(rule => rule.Conditions.Any(c => c.SensorId == sensorId))
            .Where(rule => rule.IsActive) 
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AutomationRuleEntity>> GetAllRulesAsync(
        BaseSpecification<AutomationRuleEntity>? specification,
        int? skip,
        int? take,
        CancellationToken cancellationToken)
    {
        var query = Context.Rules
            .Include(x => x.Conditions)
            .AsNoTracking();

        if (specification is not null)
        {
            query = query.Where(specification.Criteria);
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip(skip ?? 0)
            .Take(take ?? 50)
            .ToListAsync(cancellationToken);
    }
}

