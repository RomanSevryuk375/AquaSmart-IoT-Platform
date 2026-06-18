using Contracts.Enums;
using Device.Domain.ValueObjects;

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

    public override SensorType Type => SensorType.Voltage;
    public override string Unit => "V";
}
