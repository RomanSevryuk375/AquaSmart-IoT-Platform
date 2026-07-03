using Contracts.Abstractions;
using Contracts.Results;
using Control.Application.Interfaces;
using Control.Domain.Entities;
using MediatR;

namespace Control.Application.Features.Common.Behaviors;

public sealed class EcosystemSecurityBehavior<TRequest, TResponse>(ISecureService secureService)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>, IEcosystemBoundRequest
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        Result<EcosystemEntity> hasAccess = await secureService.EnsureUserOwnsEcosystemAsync(
            request.EcosystemId, cancellationToken);
        if (hasAccess.IsFailure)
        {
            BehaviorHelpers.CreateFailedResult<TResponse>(hasAccess.Error);
        }

        return await next();
    }
}
