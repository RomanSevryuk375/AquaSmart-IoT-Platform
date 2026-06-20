namespace Device.Application.Features.Relays.Command.SetRelayPowerSensor;

internal sealed class SetRelayPowerSensorHandler(
    IRelayRepository relayRepository,
    ISensorRepository sensorRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<SetRelayPowerSensorCommand, Result>
{
    public async Task<Result> Handle(
        SetRelayPowerSensorCommand request,
        CancellationToken cancellationToken)
    {
        Relay? existingRelay = await relayRepository.GetByIdAsync(
            request.RelayId, cancellationToken);

        Sensor? powerSensor = await sensorRepository.GetByIdAsync(
            request.PowerSensorId, cancellationToken);
        if (powerSensor is null)
        {
            return Result.Failure(Error.NotFound<VoltageSensor>(
                    $"{nameof(Sensor)} {request.PowerSensorId} not found"));
        }

        if (existingRelay!.ControllerId != powerSensor.ControllerId)
        {
            return Result.Failure(Error.Validation<Relay>(
                "Sensor and Relay must belong to the same controller"));
        }

        existingRelay.SetPowerSensor(powerSensor);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
