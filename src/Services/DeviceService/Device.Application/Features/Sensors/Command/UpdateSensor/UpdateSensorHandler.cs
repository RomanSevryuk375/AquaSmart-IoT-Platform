using Device.Application.Interfaces;

namespace Device.Application.Features.Sensors.Command.UpdateSensor;

public sealed class UpdateSensorHandler(
    ISensorRepository sensorRepository,
    IDeviceSecurityService securityService,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateSensorCommand, Result>
{
    public async Task<Result> Handle(
        UpdateSensorCommand request,
        CancellationToken cancellationToken)
    {
        Sensor? existingSensor = await sensorRepository.GetByIdAsync(
            request.SensorId, cancellationToken);
        if (request.ControllerId != existingSensor!.ControllerId)
        {
            Result newControllerOwnership = await securityService.EnsureUserOwnsControllerAsync(
                request.ControllerId, cancellationToken);

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

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
