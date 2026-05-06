using Control.Domain.Entities;
using Control.Domain.Interfaces;

namespace Control.Infrastructure.Repositories;

public sealed class RuleConditionRepository(ControlDbContext dbContext) 
    : BaseRepository<RuleConditionEntity>(dbContext), IRuleConditionRepository
{
}
