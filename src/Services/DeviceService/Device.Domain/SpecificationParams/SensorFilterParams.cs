using Contracts.Enums;

namespace Device.Domain.SpecificationParams;

public sealed class SensorFilterParams
{
    public Guid? ControllerId { get; init; }
    public SensorTypeEnum? Type { get; init; }
    public SensorStateEnum? State { get; init; }
}
