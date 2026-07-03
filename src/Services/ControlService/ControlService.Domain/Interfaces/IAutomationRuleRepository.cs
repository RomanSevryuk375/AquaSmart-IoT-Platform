using Contracts.Abstractions;
using Control.Domain.Entities;

namespace Control.Domain.Interfaces;

public interface IAutomationRuleRepository : IRepository<AutomationRuleEntity>
{
    public Task<AutomationRuleEntity?> GetByIdWithConditionsAsync(
        Guid id,
        CancellationToken cancellationToken);

    public Task<IReadOnlyList<AutomationRuleEntity>> GetBySensorIdWithConditionsAsync(
        Guid sensorId,
        CancellationToken cancellationToken);
}