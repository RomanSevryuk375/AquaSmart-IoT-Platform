// Ignore Spelling: Dto

namespace Notification.Application.Features.MaintenanceLogs.Queries.Shared;

public sealed record MaintenanceLogDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid EcosystemId { get; init; }
    public DateTime ActionDate { get; init; }
    public Dictionary<string, double> Metrics { get; init; } = [];
    public string Notes { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
