using Contracts.Results;
using Notification.Application.DTOs.MaintenanceLog;

namespace Notification.Application.Interfaces;

public interface IMaintenanceLogService
{
    public Task<Result<MaintenanceLogResponseDto>> GetLogById(
        Guid logId,
        CancellationToken cancellationToken);

    public Task<Result<Guid>> AddLogAsync(
        MaintenanceLogRequestDto request,
        CancellationToken cancellationToken);
}
