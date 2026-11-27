using BuildingBlocks.Domain.Common;
using BuildingBlocks.Domain.Constants;
using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Domain.Results;

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
        var errors = new List<string>();

        if (id == Guid.Empty)
        {
            errors.Add(CommonErrors.IdEmpty);
        }

        if (controllerId == Guid.Empty)
        {
            errors.Add(ControllerErrors.ControllerIdEmpty);
        }

        if (userId == Guid.Empty)
        {
            errors.Add(UserErrors.UserIdEmpty);
        }

        DeviceName? name = DomainGuard.Collect(DeviceName.Create(rawName), errors);
        ConnectionAddress? address = DomainGuard.Collect(
            ConnectionAddress.Create(protocol, rawAddress), errors);

        if (!Enum.IsDefined(type))
        {
            errors.Add(SensorErrors.InvalidType);
        }

        if (errors.Count != 0)
        {
            return DomainGuard.ValidationFailure<Sensor>(errors);
        }

        DateTime now = DateTime.UtcNow;

        Sensor sensor = type switch
        {
            SensorType.Humidity => new HumiditySensor(
                id, controllerId, userId, name!, address!, now),
            SensorType.Pressure => new PressureSensor(
                id, controllerId, userId, name!, address!, now),
            SensorType.Temperature => new TemperatureSensor(
                id, controllerId, userId, name!, address!, now),
            SensorType.Voltage => new VoltageSensor(
                id, controllerId, userId, name!, address!, now),
            _ => throw new InvalidOperationException("Unreachable sensor type.")
        };

        sensor.RaiseCreatedEvent();

        return Result<Sensor>.Success(sensor);
    }
}
