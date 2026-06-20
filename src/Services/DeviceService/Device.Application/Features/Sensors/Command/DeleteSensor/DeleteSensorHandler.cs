namespace Device.Application.Features.Sensors.Command.DeleteSensor;

internal sealed class DeleteSensorHandler(
    ISensorRepository sensorRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteSensorCommand, Result>
{
    public async Task<Result> Handle(
        DeleteSensorCommand request,
        CancellationToken cancellationToken)
    {
        Sensor? existingSensor = await sensorRepository.GetByIdAsync(
            request.SensorId, cancellationToken);

        existingSensor!.MarkAsDeleted();

        await sensorRepository.DeleteAsync(request.SensorId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
