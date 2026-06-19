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

    public override SensorType Type => SensorType.Pressure;
    public override string Unit => "Pa";
}
