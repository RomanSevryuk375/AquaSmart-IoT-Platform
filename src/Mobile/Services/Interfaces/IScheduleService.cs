using Mobile.Services.Dto.Control;

namespace Mobile.Services.Interfaces;

public interface IScheduleService
{
    Task<List<ScheduleDto>> GetSchedulesAsync(Guid ecosystemId, CancellationToken cancellationToken = default);
    Task<bool> CreateScheduleAsync(CreateScheduleRequestDto dto, CancellationToken cancellationToken = default);
    Task<bool> ToggleScheduleAsync(Guid scheduleId, bool isActive, CancellationToken cancellationToken = default);
    Task<bool> DeleteScheduleAsync(Guid scheduleId, CancellationToken cancellationToken = default);
    void RemoveSchedulesForAquarium(Guid aquariumId);
}
