using Device.Application.Interfaces;
using Device.Domain.Factories;
using MassTransit;

namespace Device.Application.Features.Sensors.Command.AddSensor;

internal sealed class AddSensorHandler(
    IDeviceSecurityService securityService,
    ISensorRepository sensorRepository,
    IUnitOfWork unitOfWork,
    IUserContext userContext,
    IMapper mapper) : IRequestHandler<AddSensorCommand, Result<SensorCreatedResponse>>
{
    public async Task<Result<SensorCreatedResponse>> Handle(
        AddSensorCommand request,
        CancellationToken cancellationToken)
    {
        Result ownership = await securityService.EnsureUserOwnsControllerAsync(
            request.ControllerId, cancellationToken);
        if (ownership.IsFailure)
        {
            return Result<SensorCreatedResponse>.Failure(ownership.Error);
        }

        Result<Sensor> sensor = SensorFactory.CreateSensor(
            id: NewId.NextGuid(), request.ControllerId, userContext.UserId,
            request.Name,
            request.ConnectionProtocol, request.ConnectionAddress,
            request.Type);
        if (sensor.IsFailure)
        {
            return Result<SensorCreatedResponse>.Failure(sensor.Error);
        }

        await sensorRepository.AddAsync(sensor.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<SensorCreatedResponse>.Success(
            mapper.Map<SensorCreatedResponse>(sensor.Value));
    }
}
