namespace Notification.Domain.SpecificationParams;

public record MaintenanceLogSpecificationParams
{
    public Guid? UserId { get; init; }
    public Guid? EcosystemId { get; init; }
    public DateTime? ActionDateFrom { get; init; }
    public DateTime? ActionDateTo { get; init; }
}
