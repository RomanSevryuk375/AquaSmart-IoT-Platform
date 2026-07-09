namespace Device.Domain.Entities.Sensors;

public sealed class VoltageSensor : Sensor
{
    internal VoltageSensor(
        Guid id, Guid controllerId, Guid userId, DeviceName name,
        ConnectionAddress address, DateTime createdAt)
        : base(id, controllerId, userId, name, address, SensorType.Voltage, "V", createdAt)
    { }

#pragma warning disable CS8618
    private VoltageSensor() { }
#pragma warning restore CS8618 
}
