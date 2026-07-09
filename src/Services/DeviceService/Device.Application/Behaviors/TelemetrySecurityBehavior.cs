using Device.Application.Interfaces;

namespace Device.Application.Behaviors;

public sealed class TelemetrySecurityBehavior<TRequest, TResponse>(
    IControllerRepository controllerRepository,
    IMyHasher myHasher)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>, IMacAddressTokenBoundRequest
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        Controller? existingController = await controllerRepository.GetByMacAddressAsync(
            request.MacAddress, cancellationToken);
        if (existingController is null)
        {
            return BehaviorHelpers.CreateFailedResult<TResponse>(Error.NotFound<Controller>(
                    string.Format(ErrorMessages.ControllerNotFound, request.MacAddress)));
        }

        if (!myHasher.Verify(request.DeviceToken, existingController.DeviceTokenHash))
        {
            return BehaviorHelpers.CreateFailedResult<TResponse>(Error.Conflict(
                ErrorMessages.AccessDenied, ErrorMessages.YouAreNotOwnerOfController));
        }

        return await next();
    }
}
