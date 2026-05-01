namespace Notification.Application.DTOs.MaintenanceLog;

public class MaintenanceLogRequestDto
{
    public Guid UserId { get; init; }
    public Guid EcosystemId { get; init; }
    public DateTime ActionDate { get; init; }
    public Dictionary<string, double> Metrics { get; init; } = [];
    public string Notes { get; init; } = string.Empty;
}
