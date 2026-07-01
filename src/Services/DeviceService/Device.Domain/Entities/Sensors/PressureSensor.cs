namespace Device.Domain.Entities.Sensors;

public sealed class PressureSensor : Sensor
{
    internal PressureSensor(
        Guid id, Guid controllerId, Guid userId, DeviceName name,
        ConnectionAddress address, DateTime createdAt)
        : base(id, controllerId, userId, name, address, SensorType.Pressure, "Pa", createdAt)
    { }

#pragma warning disable CS8618
    private PressureSensor() { }
#pragma warning restore CS8618 
}
