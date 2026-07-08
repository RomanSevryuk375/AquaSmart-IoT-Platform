using Contracts.Abstractions;
using Contracts.Results;
using Notification.Application.Features.MaintenanceLogs.Queries.Shared;

namespace Notification.Application.Features.MaintenanceLogs.Queries.GetAllMaintenanceLogs;

public sealed record GetAllMaintenanceLogsQuery
    : IQuery<Result<IReadOnlyList<MaintenanceLogDto>>>
{
    public Guid UserId { get; init; }
    public Guid? EcosystemId { get; init; }
    public DateTime? ActionDateFrom { get; init; }
    public DateTime? ActionDateTo { get; init; }
    public int Skip { get; init; } = 0;
    public int Take { get; init; } = 10;
}
