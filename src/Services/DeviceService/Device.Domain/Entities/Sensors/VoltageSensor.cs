namespace Device.Domain.Entities.Sensors;

public sealed class VoltageSensor : Sensor
{
    internal VoltageSensor(
        Guid id,
        Guid controllerId,
        Guid userId,
        DeviceName name,
        ConnectionAddress address,
        DateTime createdAt) : base(id, controllerId, userId, name, address, createdAt) { }

#pragma warning disable CS8618
    public VoltageSensor() { }
#pragma warning restore CS8618 
    public override SensorType Type => SensorType.Voltage;
    public override string Unit => UnitConstants.Volt;
}
