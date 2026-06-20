namespace Device.Application.Features.Sensors.Command.SetSensorState;

internal sealed class SetSensorStateHandler(
    ISensorRepository sensorRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<SetSensorStateCommand, Result>
{
    public async Task<Result> Handle(
        SetSensorStateCommand request,
        CancellationToken cancellationToken)
    {
        Sensor? existingSensor = await sensorRepository.GetByIdAsync(
            request.SensorId, cancellationToken);

        existingSensor!.SetState(request.SensorState);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
