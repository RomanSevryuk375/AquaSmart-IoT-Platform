using Contracts.Abstractions;
using Notification.Application.Interfaces;

namespace Notification.Application.Features.MaintenanceLogs.Commands.CreateMaintenanceLog;

public sealed record CreateMaintenanceLogCommand
    : ICommand<Guid>, IEcosystemBoundRequest
{
    public Guid UserId { get; init; }
    public Guid EcosystemId { get; init; }
    public DateTime ActionDate { get; init; }
    public Dictionary<string, double> Metrics { get; init; } = [];
    public string Notes { get; init; } = string.Empty;
}
