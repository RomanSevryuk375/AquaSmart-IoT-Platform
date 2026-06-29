namespace Device.Domain.Factories;

public static class SensorFactory
{
    public static Result<Sensor> CreateSensor(
        Guid id,
        Guid controllerId,
        Guid userId,
        string rawName,
        ConnectionProtocol protocol,
        string rawAddress,
        SensorType type)
    {
        Result<DeviceName> nameResult = DeviceName.Create(rawName);
        if (nameResult.IsFailure)
        {
            return Result<Sensor>.Failure(nameResult.Error);
        }

        Result<ConnectionAddress> addressResult = ConnectionAddress.Create(protocol, rawAddress);
        if (addressResult.IsFailure)
        {
            return Result<Sensor>.Failure(addressResult.Error);
        }

        if (id == Guid.Empty)
        {
            return Result<Sensor>.Failure(Error.Validation<Sensor>(
                CommonErrors.IdEmpty));
        }

        if (controllerId == Guid.Empty)
        {
            return Result<Sensor>.Failure(Error.Validation<Sensor>(
                ControllerErrors.ControllerIdEmpty));
        }

        if (userId == Guid.Empty)
        {
            return Result<Sensor>.Failure(Error.Validation<Sensor>(
                UserErrors.UserIdEmpty));
        }

        DateTime now = DateTime.UtcNow;

        Sensor sensor = type switch
        {
            SensorType.Humidity => new HumiditySensor(
                id, controllerId, userId, nameResult.Value, addressResult.Value, now),
            SensorType.Pressure => new PressureSensor(
                id, controllerId, userId, nameResult.Value, addressResult.Value, now),
            SensorType.Temperature => new TemperatureSensor(
                id, controllerId, userId, nameResult.Value, addressResult.Value, now),
            SensorType.Voltage => new VoltageSensor(
                id, controllerId, userId, nameResult.Value, addressResult.Value, now),

            _ => null!
        };

        if (sensor is null)
        {
            return Result<Sensor>.Failure(Error.Validation<Sensor>(
                SensorErrors.InvalidType));
        }

        sensor.RaiseCreatedEvent();

        return Result<Sensor>.Success(sensor);
    }
}
