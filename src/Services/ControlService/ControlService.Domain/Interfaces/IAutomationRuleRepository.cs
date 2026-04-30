using Contracts.Abstractions;
using Control.Domain.Entities;

namespace Control.Domain.Interfaces;

public interface IAutomationRuleRepository : IRepository<AutomationRuleEntity>
{
    Task<IReadOnlyList<AutomationRuleEntity>?> GetBySensorIdAsync(
        Guid sensorId, 
        CancellationToken cancellationToken);
}