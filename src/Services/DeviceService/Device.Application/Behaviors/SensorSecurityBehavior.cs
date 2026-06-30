using Device.Application.Interfaces;

namespace Device.Application.Behaviors;

public sealed class SensorSecurityBehavior<TRequest, TResponse>(
    ISensorRepository sensorRepository,
    IDeviceSecurityService securityService)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>, ISensorBoundRequest
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        Sensor? existingSensor = await sensorRepository.GetByIdAsync(
            request.SensorId, cancellationToken);
        if (existingSensor is null)
        {
            return BehaviorHelpers.CreateFailedResult<TResponse>(Error.NotFound<Sensor>(
                    string.Format(ErrorMessages.SensorNotFound, request.SensorId)));
        }

        Result ownership = await securityService.EnsureUserOwnsControllerAsync(
            existingSensor.ControllerId, cancellationToken);
        if (ownership.IsFailure)
        {
            return BehaviorHelpers.CreateFailedResult<TResponse>(ownership.Error);
        }

        return await next();
    }
}
