using Contracts.Enums;

namespace Device.Domain.SpecificationParams;

public sealed class SensorFilterParams
{
    public Guid? ControllerId { get; init; }
    public Guid? UserId { get; init; }
    public SensorType? Type { get; init; }
    public SensorState? State { get; init; }
}
