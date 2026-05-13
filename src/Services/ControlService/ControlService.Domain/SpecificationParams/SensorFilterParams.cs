using Contracts.Enums;

namespace Control.Domain.SpecificationParams;

public sealed record SensorFilterParams
{
    public Guid? EcosystemId { get; init; }
    public Guid? ControllerId { get; init; }
    public SensorStateEnum? State { get; init; }
    public SensorTypeEnum? Type { get; init; }
}
