using Device.Application.Interfaces;

namespace Device.Application.Behaviors;

internal class TelemetrySecurityBehavior<TRequest, TResponse>(
    IControllerRepository controllerRepository,
    IMyHasher myHasher)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>, IMacAddressTokenBoudRequest
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
                    $"{nameof(Controller)} {request.MacAddress} not found"));
        }

        if (!myHasher.Verify(request.DeviceToken, existingController.DeviceTokenHash))
        {
            return BehaviorHelpers.CreateFailedResult<TResponse>(Error.Conflict(
                "Access.Denied", "You are not the owner of this controller"));
        }

        return await next();
    }
}
