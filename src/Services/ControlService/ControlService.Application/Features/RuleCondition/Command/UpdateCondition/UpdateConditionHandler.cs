using Contracts.Results;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using MediatR;

namespace Control.Application.Features.RuleCondition.Command.UpdateCondition;

public sealed class UpdateConditionHandler(
    IAutomationRuleRepository ruleRepository,
    IRuleConditionRepository ruleConditionRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateConditionCommand, Result>
{
    public async Task<Result> Handle(
        UpdateConditionCommand request,
        CancellationToken cancellationToken)
    {
        AutomationRuleEntity? rule = await ruleRepository.GetByIdWithConditionsAsync(
            request.RuleId, cancellationToken);
        if (rule is null)
        {
            return Result.Failure(Error.NotFound(
                    "Rule.NotFound", $"Rule {request.RuleId} not found"));
        }

        Result<RuleConditionEntity> conditionOwnership = await EnsureRuleOwnsConditionAsync(
            rule, request.ConditionId, cancellationToken);
        if (conditionOwnership.IsFailure)
        {
            return Result.Failure(conditionOwnership.Error);
        }

        Result updateResult = conditionOwnership.Value.Update(
            request.Condition, request.Threshold, request.Hysteresis);
        if (updateResult.IsFailure)
        {
            return Result.Failure(updateResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<Result<RuleConditionEntity>> EnsureRuleOwnsConditionAsync(
        AutomationRuleEntity rule,
        Guid conditionId,
        CancellationToken cancellationToken)
    {
        RuleConditionEntity? condition = await ruleConditionRepository.GetByIdAsync(
            conditionId, cancellationToken);
        if (condition is null)
        {
            return Result<RuleConditionEntity>
                .Failure(Error.NotFound(
                    "Condition.NotFound",
                    "Condition not found"));
        }

        if (condition.AutomationRuleId != rule.Id)
        {
            return Result<RuleConditionEntity>
                .Failure(Error.Conflict(
                    "Condition.Forbidden",
                    "Rule do not contains this condition"));
        }

        return Result<RuleConditionEntity>.Success(condition);
    }
}
