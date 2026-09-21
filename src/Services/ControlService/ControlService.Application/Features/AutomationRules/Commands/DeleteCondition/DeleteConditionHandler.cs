using BuildingBlocks.Domain.Results;
using Control.Application.Constants;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using MediatR;
using ZiggyCreatures.Caching.Fusion;

namespace Control.Application.Features.AutomationRules.Commands.DeleteCondition;

public sealed class DeleteConditionHandler(
    IAutomationRuleRepository ruleRepository,
    IFusionCache cache)
    : IRequestHandler<DeleteConditionCommand, Result>
{
    public async Task<Result> Handle(
        DeleteConditionCommand request,
        CancellationToken cancellationToken)
    {
        AutomationRule? rule = await ruleRepository.GetByIdWithConditionsAsync(
            request.RuleId, cancellationToken);
        if (rule is null)
        {
            return Result.Failure(Error.NotFound<AutomationRule>(
                $"Rule {request.RuleId} not found"));
        }

        RuleCondition? condition = rule.Conditions.FirstOrDefault(x => x.Id == request.ConditionId);
        if (condition is null)
        {
            return Result.Failure(Error.NotFound<RuleCondition>(
                $"Condition {request.ConditionId} not found."));
        }

        Result removeResult = rule.RemoveCondition(condition);
        if (removeResult.IsFailure)
        {
            return Result.Failure(removeResult.Error);
        }

        await cache.RemoveAsync(CacheKeys.Rule(request.UserId, request.RuleId), token: cancellationToken);

        return Result.Success();
    }
}
