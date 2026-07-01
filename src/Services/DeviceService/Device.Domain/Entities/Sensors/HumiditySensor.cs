namespace Device.Domain.Entities.Sensors;

public sealed class HumiditySensor : Sensor
{
    internal HumiditySensor(
        Guid id, Guid controllerId, Guid userId, DeviceName name,
        ConnectionAddress address, DateTime createdAt)
        : base(id, controllerId, userId, name, address, SensorType.Humidity, "%", createdAt)
    { }

#pragma warning disable CS8618
    private HumiditySensor() { }
#pragma warning restore CS8618 
}
