using Contracts.Abstractions;
using Contracts.Results;
using Notification.Application.Features.MaintenanceLogs.Queries.Shared;

namespace Notification.Application.Features.MaintenanceLogs.Queries.GetMaintenanceLogById;

public sealed record GetMaintenanceLogByIdQuery
    : IQuery<Result<MaintenanceLogDto>>
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
}
