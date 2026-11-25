using BuildingBlocks.Domain.Results;
using Device.Application.Interfaces;

namespace Device.Application.Features.Sensors.Command.UpdateSensor;

public sealed class UpdateSensorHandler(
    ISensorRepository sensorRepository,
    IControllerRepository controllerRepository,
    IDeviceSecurityService securityService) : IRequestHandler<UpdateSensorCommand, Result>
{
    public async Task<Result> Handle(
        UpdateSensorCommand request,
        CancellationToken cancellationToken)
    {
        Sensor? existingSensor = await sensorRepository.GetByIdAsync(
            request.SensorId, cancellationToken);

        Guid previousControllerId = existingSensor!.ControllerId;

        if (request.ControllerId != previousControllerId)
        {
            Result newControllerOwnership = await securityService.EnsureUserOwnsControllerAsync(
                request.ControllerId, request.UserId, cancellationToken);

            if (newControllerOwnership.IsFailure)
            {
                return newControllerOwnership;
            }
        }

        Result result = existingSensor.Update(
            request.ControllerId,
            request.Name, request.ConnectionProtocol, request.ConnectionAddress);

        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        Controller? targetController = await controllerRepository.GetByIdAsync(request.ControllerId, cancellationToken);
        targetController?.IncrementConfigVersion("sensor_updated");

        if (previousControllerId != request.ControllerId)
        {
            Controller? previousController = await controllerRepository.GetByIdAsync(previousControllerId, cancellationToken);
            previousController?.IncrementConfigVersion("sensor_updated");
        }

        return Result.Success();
    }
}
