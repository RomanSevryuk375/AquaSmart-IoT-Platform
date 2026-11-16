using Mobile.Services.Dto.Notification;

namespace Mobile.Services.Interfaces;

public interface IMaintenanceService
{
    Task<List<MaintenanceLogDto>> GetMaintenanceLogsAsync(Guid ecosystemId, CancellationToken cancellationToken = default);
    Task<bool> AddMaintenanceLogAsync(CreateMaintenanceLogRequestDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteMaintenanceLogAsync(Guid logId, CancellationToken cancellationToken = default);
    void RemoveMaintenanceLogsForAquarium(Guid aquariumId);
}
