using System.Data;
using BuildingBlocks.Application.Behaviors;
using BuildingBlocks.Domain.Constants;
using BuildingBlocks.Domain.Results;
using Control.Application.Interfaces;
using Control.Domain.Entities;
using Control.Domain.Interfaces;
using MediatR;

namespace Control.Application.Behaviors;

public sealed class AutomationRuleSecurityBehavior<TRequest, TResponse>(
    IAutomationRuleRepository ruleRepository,
    IEcosystemRepository ecosystemRepository)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>, IRuleBoundRequest
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        AutomationRule? rule = await ruleRepository.GetByIdAsync(request.RuleId, cancellationToken);
        if (rule is null)
        {
            return BehaviorHelpers.CreateFailedResult<TResponse>(Error.NotFound<Rule>(
                $"Rule {request.RuleId} not found"));
        }

        Ecosystem? ecosystem = await ecosystemRepository.GetByIdAsync(rule.EcosystemId, cancellationToken);
        if (ecosystem is null)
        {
            return BehaviorHelpers.CreateFailedResult<TResponse>(Error.NotFound<Ecosystem>(
                    $"Ecosystem {rule.EcosystemId} not found. "));
        }

        if (ecosystem.UserId != request.UserId)
        {
            return BehaviorHelpers.CreateFailedResult<TResponse>(Error.Forbidden(
                    ErrorCodes.Security.AccessDenied,
                    ErrorMessages.Security.YouAreNotOwnerOfEcosystem));
        }

        return await next(cancellationToken);
    }
}
