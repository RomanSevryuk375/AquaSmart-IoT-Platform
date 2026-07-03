using Contracts.Results;
using Control.Domain.Interfaces;
using MediatR;

namespace Control.Application.Features.RuleConditions.Command.DeleteCondition;

public sealed class DeleteConditionHandler(
    IAutomationRuleRepository ruleRepository,
    IRuleConditionRepository ruleConditionRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteConditionCommand, Result>
{
    public async Task<Result> Handle(
        DeleteConditionCommand request,
        CancellationToken cancellationToken)
    {
        Domain.Entities.AutomationRule? rule = await ruleRepository.GetByIdWithConditionsAsync(
            request.RuleId, cancellationToken);
        if (rule is null)
        {
            return Result<Guid>.Failure(Error.NotFound(
                    "Rule.NotFound", $"Rule {request.RuleId} not found"));
        }

        Result<Domain.Entities.RuleCondition> conditionOwnership = await EnsureRuleOwnsConditionAsync(
            rule, request.ConditionId, cancellationToken);
        if (conditionOwnership.IsFailure)
        {
            return Result.Failure(conditionOwnership.Error);
        }

        await ruleConditionRepository.DeleteAsync(request.ConditionId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<Result<Domain.Entities.RuleCondition>> EnsureRuleOwnsConditionAsync(
        Domain.Entities.AutomationRule rule,
        Guid conditionId,
        CancellationToken cancellationToken)
    {
        Domain.Entities.RuleCondition? condition = await ruleConditionRepository.GetByIdAsync(
            conditionId, cancellationToken);
        if (condition is null)
        {
            return Result<Domain.Entities.RuleCondition>
                .Failure(Error.NotFound(
                    "Condition.NotFound",
                    "Condition not found"));
        }

        if (condition.AutomationRuleId != rule.Id)
        {
            return Result<Domain.Entities.RuleCondition>
                .Failure(Error.Conflict(
                    "Condition.Forbidden",
                    "Rule do not contains this condition"));
        }

        return Result<Domain.Entities.RuleCondition>.Success(condition);
    }
}
