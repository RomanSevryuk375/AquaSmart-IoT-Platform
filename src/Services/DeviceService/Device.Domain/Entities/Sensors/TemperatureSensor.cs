namespace Device.Domain.Entities.Sensors;

public sealed class TemperatureSensor : Sensor
{
    internal TemperatureSensor(
        Guid id,
        Guid controllerId,
        Guid userId,
        DeviceName name,
        ConnectionAddress address,
        DateTime createdAt) : base(id, controllerId, userId, name, address, createdAt) { }

#pragma warning disable CS8618
    public TemperatureSensor() { }
#pragma warning restore CS8618 
    public override SensorType Type => SensorType.Temperature;
    public override string Unit => "°C";
}
