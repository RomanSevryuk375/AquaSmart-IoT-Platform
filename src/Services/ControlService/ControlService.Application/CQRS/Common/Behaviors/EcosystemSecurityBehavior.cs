using Contracts.Abstractions;
using Control.Application.Interfaces;
using MediatR;

namespace Control.Application.CQRS.Common.Behaviors;

public sealed class EcosystemSecurityBehavior<TRequest, TResponse>(ISecureService secureService)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>, IEcosystemBoundRequest
{
    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        var hasAccess = await secureService.EnsureUserOwnsEcosystemAsync(
            request.EcosystemId, cancellationToken);
        if(hasAccess.IsFailure)
        {
            BehaviorHelpers.CreateFailedResult<TResponse>(hasAccess.Error);
        }

        return await next();
    }
}
