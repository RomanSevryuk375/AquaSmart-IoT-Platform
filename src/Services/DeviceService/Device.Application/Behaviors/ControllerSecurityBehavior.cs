using Device.Application.Interfaces;

namespace Device.Application.Behaviors;

internal sealed class ControllerSecurityBehavior<TRequest, TResponse>(
    IDeviceSecurityService securityService)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>, IControllerBoundRequest
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        Result ownership = await securityService.EnsureUserOwnsControllerAsync(
             request.ControllerId, cancellationToken);
        if (ownership.IsFailure)
        {
            return BehaviorHelpers.CreateFailedResult<TResponse>(ownership.Error);
        }

        return await next();
    }
}
