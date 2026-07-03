using Control.Domain.Entities;
using Control.Domain.Interfaces;

namespace Control.Infrastructure.Persistence.Repositories;

public sealed class RuleConditionRepository(SystemDbContext dbContext)
    : BaseRepository<RuleCondition>(dbContext), IRuleConditionRepository
{
}
