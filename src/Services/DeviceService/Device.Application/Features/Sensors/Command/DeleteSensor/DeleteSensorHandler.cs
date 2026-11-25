using BuildingBlocks.Domain.Results;

namespace Device.Application.Features.Sensors.Command.DeleteSensor;

public sealed class DeleteSensorHandler(
    ISensorRepository sensorRepository,
    IControllerRepository controllerRepository)
    : IRequestHandler<DeleteSensorCommand, Result>
{
    public async Task<Result> Handle(
        DeleteSensorCommand request,
        CancellationToken cancellationToken)
    {
        Sensor? existingSensor = await sensorRepository.GetByIdAsync(
            request.SensorId, cancellationToken);

        Guid controllerId = existingSensor!.ControllerId;
        existingSensor.MarkAsDeleted();

        await sensorRepository.DeleteAsync(request.SensorId, cancellationToken);

        Controller? controller = await controllerRepository.GetByIdAsync(controllerId, cancellationToken);
        controller?.IncrementConfigVersion("sensor_deleted");

        return Result.Success();
    }
}
