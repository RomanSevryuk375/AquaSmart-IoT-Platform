
using Device.Application.Interfaces;

namespace Device.Application.Behaviors;

internal sealed class RelaySecurityBehavior<TRequest, TResponse>(
    IRelayRepository relayRepository,
    IDeviceSecurityService securityService)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>, IRelayBoundRequest
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        Relay? existingRelay = await relayRepository.GetByIdAsync(
            request.RelayId, cancellationToken);
        if (existingRelay is null)
        {
            return BehaviorHelpers.CreateFailedResult<TResponse>(Error.NotFound<Relay>(
                $"{nameof(Relay)} {request.RelayId} not found"));
        }

        Result ownership = await securityService.EnsureUserOwnsControllerAsync(
            existingRelay.ControllerId, cancellationToken);
        if (ownership.IsFailure)
        {
            return BehaviorHelpers.CreateFailedResult<TResponse>(ownership.Error);
        }

        return await next();
    }
}
