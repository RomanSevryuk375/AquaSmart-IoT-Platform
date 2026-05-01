using Contracts.Results;
using Control.Application.DTOs.AutomationRule;

namespace Control.Application.Interfaces;

public interface IRuleConditionService
{
    Task<Result<Guid>> AddConditionAsync(
        Guid ruleId, 
        RuleConditionRequestDto request, 
        CancellationToken cancellationToken);

    Task<Result> DeleteConditionAsync(
        Guid ruleId, 
        Guid conditionId, 
        CancellationToken cancellationToken);

    Task<Result> UpdateConditionAsync(
        Guid ruleId, 
        Guid conditionId, 
        RuleConditionRequestDto request, 
        CancellationToken cancellationToken);
}