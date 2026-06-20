using Device.Application.Interfaces;

namespace Device.Application.Features.Sensors.Command.SetSensorState;

internal sealed class SetSensorStateHandler(
    ISensorRepository sensorRepository,
    IDeviceSecurityService securityService,
    IUnitOfWork unitOfWork) : IRequestHandler<SetSensorStateCommand, Result>
{
    public async Task<Result> Handle(
        SetSensorStateCommand request,
        CancellationToken cancellationToken)
    {
        Sensor? existingSensor = await sensorRepository
           .GetByIdAsync(request.SensorId, cancellationToken);

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

        existingSensor.SetState(request.SensorState);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
