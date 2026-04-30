using Control.Domain.Entities;
using Control.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Control.Infrastructure.Repositories;

public sealed class AutomationRuleRepository(SystemDbContext dbContext)
        : BaseRepository<AutomationRuleEntity>(dbContext), IAutomationRuleRepository
{
    public async Task<IReadOnlyList<AutomationRuleEntity>?> GetBySensorIdAsync(
        Guid sensorId, 
        CancellationToken cancellationToken)
    {
        return await Context.Rules
            .AsNoTracking()
            .Where(x => x.Conditions
                .Any(x => x.SensorId == sensorId))
            .ToListAsync(cancellationToken);
    }
}

