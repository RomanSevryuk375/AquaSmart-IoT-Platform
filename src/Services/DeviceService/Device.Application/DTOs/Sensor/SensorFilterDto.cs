using Contracts.Enums;

namespace Device.Application.DTOs.Sensor;

public sealed record SensorFilterDto
{
    public Guid? ControllerId { get; init; }
    public SensorTypeEnum? Type { get; init; }
    public SensorStateEnum State { get; init; }
}
