using BuildingBlocks.Domain.Results;
using Control.Application.Constants;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using MediatR;
using ZiggyCreatures.Caching.Fusion;

namespace Control.Application.Features.AutomationRules.Commands.UpdateRule;

public sealed class UpdateRuleHandler(
    IAutomationRuleRepository ruleRepository,
    IFusionCache cache)
    : IRequestHandler<UpdateRuleCommand, Result>
{
    public async Task<Result> Handle(
        UpdateRuleCommand request,
        CancellationToken cancellationToken)
    {
        AutomationRule? rule = await ruleRepository.GetByIdAsync(request.RuleId, cancellationToken);
        if (rule is null)
        {
            return Result.Failure(Error.NotFound<AutomationRule>(
                $"Rule {request.RuleId} not found"));
        }

        Result validationResult = rule.Update(
            request.Name, request.RelayId, request.Operator, request.Action);
        if (validationResult.IsFailure)
        {
            return Result.Failure(validationResult.Error);
        }

        await cache.RemoveAsync(CacheKeys.Rule(request.UserId, request.RuleId), token: cancellationToken);

        return Result.Success();
    }
}
