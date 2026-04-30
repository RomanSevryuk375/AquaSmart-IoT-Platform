namespace Control.Domain.SpecificationParams;

public record VacationModeFilterParams
{
    public Guid? EcosystemId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public bool? IsActive { get; init; }
}
