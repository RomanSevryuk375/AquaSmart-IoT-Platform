using Contracts.Abstractions;
using Control.Domain.Entities;

namespace Control.Domain.Interfaces;

public interface IAutomationRuleRepository : IRepository<AutomationRule>
{
    public Task<AutomationRule?> GetByIdWithConditionsAsync(
        Guid id,
        CancellationToken cancellationToken);

    public Task<IReadOnlyList<AutomationRule>> GetBySensorIdWithConditionsAsync(
        Guid sensorId,
        CancellationToken cancellationToken);
}