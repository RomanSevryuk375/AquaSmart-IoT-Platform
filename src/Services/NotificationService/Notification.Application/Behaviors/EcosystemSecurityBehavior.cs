using BuildingBlocks.Application.Behaviors;
using BuildingBlocks.Domain.Constants;
using BuildingBlocks.Domain.Results;
using MediatR;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;

namespace Notification.Application.Behaviors;

public sealed class EcosystemSecurityBehavior<TRequest, TResponse>(
    IEcosystemRepository ecosystemRepository)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>, IEcosystemBoundRequest
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        Ecosystem? ecosystem = await ecosystemRepository.GetByIdAsync(
            request.EcosystemId, cancellationToken);
        if (ecosystem is null)
        {
            return BehaviorHelpers.CreateFailedResult<TResponse>(Error.NotFound<Ecosystem>(
                    $"Ecosystem {request.EcosystemId} not found. "));
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
