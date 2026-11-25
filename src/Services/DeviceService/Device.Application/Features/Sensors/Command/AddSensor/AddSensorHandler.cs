using BuildingBlocks.Domain.Results;
using Device.Domain.Factories;
using MassTransit;

namespace Device.Application.Features.Sensors.Command.AddSensor;

public sealed class AddSensorHandler(
    ISensorRepository sensorRepository,
    IControllerRepository controllerRepository,
    IMapper mapper) : IRequestHandler<AddSensorCommand, Result<SensorCreatedResponse>>
{
    public async Task<Result<SensorCreatedResponse>> Handle(
        AddSensorCommand request,
        CancellationToken cancellationToken)
    {
        Result<Sensor> sensor = SensorFactory.CreateSensor(
            id: NewId.NextGuid(), request.ControllerId, request.UserId,
            request.Name,
            request.ConnectionProtocol, request.ConnectionAddress,
            request.Type);
        if (sensor.IsFailure)
        {
            return Result<SensorCreatedResponse>.Failure(sensor.Error);
        }

        await sensorRepository.AddAsync(sensor.Value, cancellationToken);

        Controller? controller = await controllerRepository.GetByIdAsync(request.ControllerId, cancellationToken);
        controller?.IncrementConfigVersion("sensor_added");

        return Result<SensorCreatedResponse>.Success(
            mapper.Map<SensorCreatedResponse>(sensor.Value));
    }
}
