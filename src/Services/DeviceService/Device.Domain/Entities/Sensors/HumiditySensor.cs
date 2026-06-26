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

#pragma warning disable CS8618
    public HumiditySensor() { }
#pragma warning restore CS8618 

    public override SensorType Type => SensorType.Humidity;
    public override string Unit => UnitConstants.Percent;
}
