using Contracts.Enums;

namespace Device.Application.DTOs.Sensor;

public sealed record SensorFilterDto
{
    public Guid? ControllerId { get; init; }
    public SensorType? Type { get; init; }
    public SensorState State { get; init; }
}
