using Contracts.Abstractions;
using Contracts.Results;
using Control.Application.Interfaces;
using Control.Domain.Interfaces;
using MediatR;

namespace Control.Application.Features.Common.Behaviors;

public sealed class AutomationRuleSecurityBehavior<TRequest, TResponse>(
    ISecureService secureService, IAutomationRuleRepository ruleRepository)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>, IRuleBoundRequest
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        Domain.Entities.AutomationRule? rule = await ruleRepository.GetByIdAsync(request.RuleId, cancellationToken);
        if (rule is null)
        {
            var notFoundError = Error.NotFound("Rule.NotFound", $"Rule {request.RuleId} not found");
            return BehaviorHelpers.CreateFailedResult<TResponse>(notFoundError);
        }

        Result<Domain.Entities.Ecosystem> hasAccess = await secureService.EnsureUserOwnsEcosystemAsync(
            rule.EcosystemId, cancellationToken);
        if (hasAccess.IsFailure)
        {
            BehaviorHelpers.CreateFailedResult<TResponse>(hasAccess.Error);
        }

        return await next();
    }
}
