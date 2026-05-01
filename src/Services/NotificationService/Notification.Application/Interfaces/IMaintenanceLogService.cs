using Contracts.Results;
using Notification.Application.DTOs.MaintenanceLog;

namespace Notification.Application.Interfaces;

public interface IMaintenanceLogService
{
    Task<Result<IReadOnlyList<MaintenanceLogResponseDto>>> GetAllLogs(
        MaintenanceLogFilterDto filter,
        int? skip,
        int? take,
        CancellationToken cancellationToken);

    Task<Result<MaintenanceLogResponseDto>> GetLogById(
        Guid logId,
        CancellationToken cancellationToken);

    Task<Result<Guid>> AddLogAsync(
        MaintenanceLogRequestDto request,
        CancellationToken cancellationToken);
}
