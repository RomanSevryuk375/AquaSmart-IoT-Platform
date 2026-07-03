namespace Control.Domain.SpecificationParams;

public sealed record ScheduleFilterParams
{
    public Guid? EcosystemId { get; init; }
    public Guid? RelayId { get; init; }
    public bool? IsFadeMode { get; init; }
    public bool? IsEnable { get; init; }
}
