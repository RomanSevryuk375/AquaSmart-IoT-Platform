namespace Device.Domain.Entities.Sensors;

public sealed class PressureSensor : Sensor
{
    internal PressureSensor(
        Guid id,
        Guid controllerId,
        Guid userId,
        DeviceName name,
        ConnectionAddress address,
        DateTime createdAt) : base(id, controllerId, userId, name, address, createdAt) { }

#pragma warning disable CS8618
    public PressureSensor() { }
#pragma warning restore CS8618 
    public override SensorType Type => SensorType.Pressure;
    public override string Unit => UnitConstants.Pascal;
}
