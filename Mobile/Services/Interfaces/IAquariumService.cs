using Mobile.Services.Dto.Control;
using Mobile.Services.Dto.Notification;

namespace Mobile.Services.Interfaces;

public interface IAquariumService
{
    Task<List<AquariumModel>> GetAquariumsAsync(CancellationToken cancellationToken = default);
    Task<AquariumModel?> GetAquariumByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AquariumModel> AddAquariumAsync(AquariumModel model, CancellationToken cancellationToken = default);
    Task<AquariumModel> UpdateAquariumAsync(AquariumModel model, CancellationToken cancellationToken = default);
    Task<bool> DeleteAquariumAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<SensorModel>> GetSensorsAsync(Guid aquariumId, CancellationToken cancellationToken = default);
    Task<SensorModel> AddSensorAsync(SensorModel sensor, CancellationToken cancellationToken = default);
    Task<List<RelayModel>> GetRelaysAsync(Guid aquariumId, CancellationToken cancellationToken = default);
    Task<RelayModel> AddRelayAsync(RelayModel relay, CancellationToken cancellationToken = default);
    Task<bool> ToggleRelayAsync(Guid relayId, CancellationToken cancellationToken = default);
    Task<List<ScenarioModel>> GetScenariosAsync(Guid aquariumId, CancellationToken cancellationToken = default);
    Task<ScenarioModel> AddScenarioAsync(ScenarioModel scenario, CancellationToken cancellationToken = default);
    Task<bool> ToggleScenarioAsync(Guid scenarioId, CancellationToken cancellationToken = default);

    Task<List<VacationModeDto>> GetVacationModesAsync(Guid ecosystemId, CancellationToken cancellationToken = default);
    Task<bool> CreateVacationModeAsync(CreateVacationModeRequestDto dto, CancellationToken cancellationToken = default);
    Task<bool> ToggleVacationModeAsync(Guid vacationModeId, CancellationToken cancellationToken = default);
    Task<bool> DeleteVacationModeAsync(Guid vacationModeId, CancellationToken cancellationToken = default);

    Task<List<MaintenanceLogDto>> GetMaintenanceLogsAsync(Guid ecosystemId, CancellationToken cancellationToken = default);
    Task<bool> AddMaintenanceLogAsync(CreateMaintenanceLogRequestDto dto, CancellationToken cancellationToken = default);

    Task<List<ScheduleDto>> GetSchedulesAsync(Guid ecosystemId, CancellationToken cancellationToken = default);
    Task<bool> CreateScheduleAsync(CreateScheduleRequestDto dto, CancellationToken cancellationToken = default);
    Task<bool> ToggleScheduleAsync(Guid scheduleId, bool isActive, CancellationToken cancellationToken = default);
    Task<bool> DeleteScheduleAsync(Guid scheduleId, CancellationToken cancellationToken = default);

    Task<List<ReminderDto>> GetRemindersAsync(Guid? ecosystemId = null, CancellationToken cancellationToken = default);
    Task<bool> CreateReminderAsync(CreateReminderRequestDto dto, CancellationToken cancellationToken = default);
    Task<bool> CompleteReminderAsync(Guid reminderId, CancellationToken cancellationToken = default);
    Task<bool> DeleteReminderAsync(Guid reminderId, CancellationToken cancellationToken = default);
}
