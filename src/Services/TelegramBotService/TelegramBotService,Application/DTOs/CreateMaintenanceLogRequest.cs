namespace DTOs;

/// <summary>
/// Payload for creating a new maintenance log entry.
/// </summary>
public sealed record CreateMaintenanceLogRequest
{
    public Guid EcosystemId { get; init; }
    public DateTime ActionDate { get; init; } = DateTime.UtcNow;
    public Dictionary<string, double> Metrics { get; init; } = [];
    public string Notes { get; init; } = string.Empty;
}
