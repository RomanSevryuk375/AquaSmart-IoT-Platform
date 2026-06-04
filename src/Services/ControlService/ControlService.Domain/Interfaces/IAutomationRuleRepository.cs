using Contracts.Abstractions;
using Control.Domain.Entities;

namespace Control.Domain.Interfaces;

public interface IAutomationRuleRepository : IRepository<AutomationRuleEntity>
{
    Task<AutomationRuleEntity?> GetByIdWithConditionsAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<AutomationRuleEntity>> GetBySensorIdWithConditionsAsync(
        Guid sensorId,
        CancellationToken cancellationToken);
}