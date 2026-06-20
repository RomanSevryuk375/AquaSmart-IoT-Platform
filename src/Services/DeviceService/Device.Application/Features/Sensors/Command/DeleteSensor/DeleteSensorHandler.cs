
using Device.Application.Interfaces;

namespace Device.Application.Features.Sensors.Command.DeleteSensor;

internal sealed class DeleteSensorHandler(
    ISensorRepository sensorRepository,
    IDeviceSecurityService securityService,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteSensorCommand, Result>
{
    public async Task<Result> Handle(
        DeleteSensorCommand request,
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
            return Result.Failure(ownership.Error);
        }

        existingSensor.MarkAsDeleted();

        await sensorRepository.DeleteAsync(request.SensorId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
