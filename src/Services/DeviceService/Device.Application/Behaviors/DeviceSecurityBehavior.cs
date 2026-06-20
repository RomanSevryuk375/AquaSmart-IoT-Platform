using Device.Application.Interfaces;

namespace Device.Application.Behaviors;

internal sealed class DeviceSecurityBehavior<TRequest, TResponse>(
    IDeviceSecurityService securityService)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>, IDeviceBoundRequest
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        Result ownership = await securityService.EnsureDeviceAccessAsync(
            request.ControllerId, request.DeviceToken, cancellationToken);
        if (ownership.IsFailure)
        {
            return BehaviorHelpers.CreateFailedResult<TResponse>(ownership.Error);
        }

        return await next();
    }
}
