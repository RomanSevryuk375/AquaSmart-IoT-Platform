
using Device.Application.Features.Sensors.Command.AddSensor;
using Device.Application.Interfaces;

namespace Device.Application.Features.Sensors.Command.UpdateSensor;

internal sealed class UpdateSensorHandler(
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
        if (existingSensor is null)
        {
            return Result.Failure(Error.NotFound<Sensor>(
                    $"{nameof(Sensor)} {request.SensorId} not found"));
        }

        Result ownership = await securityService.EnsureUserOwnsControllerAsync(
            existingSensor.ControllerId, cancellationToken);
        if (ownership.IsFailure)
        {
            return Result<SensorCreatedResponse>.Failure(ownership.Error);
        }

        if (request.ControllerId != existingSensor.ControllerId)
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
            request.Name,
            request.ConnectionProtocol,
            request.ConnectionAddress);
        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
