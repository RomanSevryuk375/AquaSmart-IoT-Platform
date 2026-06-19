namespace Device.Domain.Entities.Sensors;

public sealed class HumiditySensor : Sensor
{
    internal HumiditySensor(
        Guid id,
        Guid controllerId,
        Guid userId,
        DeviceName name,
        ConnectionAddress address,
        DateTime createdAt) : base(id, controllerId, userId, name, address, createdAt) { }

    public override SensorType Type => SensorType.Humidity;
    public override string Unit => "%";
}
