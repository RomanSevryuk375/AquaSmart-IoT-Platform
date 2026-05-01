using Contracts.Results;
using Control.Application.DTOs.AutomationRule;

namespace Control.Application.Interfaces;

public interface IAutomationRuleService
{
    Task<Result<Guid>> CreateRuleAsync(
        AutomationRuleRequestDto request, 
        CancellationToken cancellationToken);

    Task<Result> DeleteRuleAsync(
        Guid ruleId, 
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<AutomationRuleResponseDto>>> GetAllRulesAsync(
        AutomationRuleFilterDto filter,
        int? skip,
        int? take,
        CancellationToken cancellationToken);

    Task<Result<AutomationRuleResponseDto>> GetRuleByIdAsync(
        Guid ruleId, 
        CancellationToken cancellationToken);

    Task<Result> UpdateRuleAsync(
        Guid ruleId, 
        AutomationRuleUpdateRequestDto request, 
        CancellationToken cancellationToken);
}