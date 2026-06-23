using Contracts.Enums;

namespace Control.Domain.SpecificationParams;

public sealed record SensorFilterParams
{
    public Guid? EcosystemId { get; init; }
    public Guid? ControllerId { get; init; }
    public SensorState? State { get; init; }
    public SensorType? Type { get; init; }
}
