using Contracts.Enums;

namespace Control.Domain.SpecificationParams;

public sealed record RelayFilterParams
{
    public Guid? EcosystemId { get; init; }
    public Guid? ControllerId { get; init; }
    public Guid? PowerSensorId { get; init; }
    public RelayPurposeEnum? Purpose { get; init; }
    public bool? IsManual { get; init; }
    public bool? IsActive { get; init; }
}
