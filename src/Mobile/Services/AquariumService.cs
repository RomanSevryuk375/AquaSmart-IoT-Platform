using Mobile.Models;
using Mobile.Services.Dto.Control;
using Mobile.Services.Dto.Device;
using Mobile.Services.Dto.Notification;
using Mobile.Services.Interfaces;

namespace Mobile.Services;

public sealed class AquariumService : IAquariumService
{
    private readonly IApiClient _apiClient;
    private readonly ISensorService _sensorService;
    private readonly IRelayService _relayService;
    private readonly IScenarioService _scenarioService;
    private readonly IVacationModeService _vacationModeService;
    private readonly IMaintenanceService _maintenanceService;
    private readonly IScheduleService _scheduleService;
    private readonly IReminderService _reminderService;

    private readonly object _syncLock = new();
    private readonly List<AquariumModel> _aquariums = new();

    public AquariumService(
        IApiClient apiClient,
        ISensorService sensorService,
        IRelayService relayService,
        IScenarioService scenarioService,
        IVacationModeService vacationModeService,
        IMaintenanceService maintenanceService,
        IScheduleService scheduleService,
        IReminderService reminderService)
    {
        _apiClient = apiClient;
        _sensorService = sensorService;
        _relayService = relayService;
        _scenarioService = scenarioService;
        _vacationModeService = vacationModeService;
        _maintenanceService = maintenanceService;
        _scheduleService = scheduleService;
        _reminderService = reminderService;
    }

    #region Aquarium Lifecycle & Management

    public async Task<List<AquariumModel>> GetAquariumsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var ecoResponse = await _apiClient.GetAsync<List<EcosystemDto>>(ApiConfiguration.Routes.Ecosystems, cancellationToken);
            if (ecoResponse.IsSuccess && ecoResponse.Data != null)
            {
                if (ecoResponse.Data.Count == 0)
                {
                    lock (_syncLock)
                    {
                        _aquariums.Clear();
                    }
                    return new List<AquariumModel>();
                }

                // Query associated controllers, sensors, relays to enrich cards
                var controllersTask = _apiClient.GetAsync<List<ControllerDto>>(ApiConfiguration.Routes.Controllers, cancellationToken);
                var sensorsTask = _apiClient.GetAsync<List<SensorDto>>(ApiConfiguration.Routes.Sensors, cancellationToken);
                var relaysTask = _apiClient.GetAsync<List<RelayDto>>(ApiConfiguration.Routes.Relays, cancellationToken);

                await Task.WhenAll(controllersTask, sensorsTask, relaysTask);

                var controllers = (await controllersTask).Data ?? new List<ControllerDto>();
                var sensors = (await sensorsTask).Data ?? new List<SensorDto>();
                var relays = (await relaysTask).Data ?? new List<RelayDto>();

                var mappedList = new List<AquariumModel>();

                foreach (var dto in ecoResponse.Data)
                {
                    var matchedController = controllers.FirstOrDefault(c => c.Id == dto.ControllerId);
                    var matchedSensors = sensors.Where(s => s.ControllerId == dto.ControllerId).ToList();
                    var matchedRelays = relays.Where(r => r.ControllerId == dto.ControllerId).ToList();

                    var tempSensor = matchedSensors.FirstOrDefault(s => s.Type == 1);
                    double? tempVal = tempSensor?.LastValue;

                    var model = new AquariumModel
                    {
                        Id = dto.Id,
                        ControllerId = dto.ControllerId,
                        Name = dto.Name,
                        VolumeLiters = dto.Volume ?? 20.0,
                        WaterType = (WaterType)(dto.Type switch
                        {
                            1 => 0, // Freshwater
                            2 => 1, // Marine
                            _ => 0
                        }),
                        Dimensions = "—",
                        SpeciesInfo = $"{LocalizationService.Instance["Tank_EcosystemPrefix"]}: {dto.Name}",
                        ControllerName = matchedController != null ? $"{LocalizationService.Instance["Controller_Code"]}: {matchedController.Name}" : LocalizationService.Instance["Tank_NoController"],
                        IsOnline = matchedController?.IsOnline ?? false,
                        Temperature = tempVal ?? 0.0,
                        TemperatureDisplay = tempVal.HasValue ? $"{tempVal.Value:F1}°C" : "-- °C",
                        SensorsCount = matchedSensors.Count > 0 ? matchedSensors.Count : _sensorService.GetSensorsCount(dto.Id),
                        RelaysCount = matchedRelays.Count > 0 ? matchedRelays.Count : _relayService.GetRelaysCount(dto.Id),
                        ScenariosCount = _scenarioService.GetScenariosCount(dto.Id),
                        Icon = dto.Type switch
                        {
                            1 => "🐟",
                            2 => "🦎",
                            3 => "🌿",
                            4 => "🌸",
                            5 => "🐜",
                            _ => "🐟"
                        },
                        TankTypeSubtitle = dto.Type switch
                        {
                            1 => $"{LocalizationService.Instance["TankType_Aquarium"]} • {dto.Volume ?? 20:F0}{LocalizationService.Instance["Unit_Liters_Short"]}",
                            2 => $"{LocalizationService.Instance["TankType_Terrarium"]} • {dto.Volume ?? 30:F0}{LocalizationService.Instance["Unit_Liters_Short"]}",
                            3 => $"{LocalizationService.Instance["TankType_Growbox"]} • {dto.Volume ?? 15:F0}{LocalizationService.Instance["Unit_Liters_Short"]}",
                            4 => $"{LocalizationService.Instance["TankType_Florarium"]} • {dto.Volume ?? 5.5:F1}{LocalizationService.Instance["Unit_Liters_Short"]}",
                            5 => $"{LocalizationService.Instance["TankType_Formicarium"]} • {dto.Volume ?? 2:F0}{LocalizationService.Instance["Unit_Liters_Short"]}",
                            _ => $"{LocalizationService.Instance["TankType_Aquarium"]} • {dto.Volume ?? 20:F0}{LocalizationService.Instance["Unit_Liters_Short"]}"
                        },
                        CardThemeColor = dto.Type switch
                        {
                            1 => "#F0F4FF",
                            2 => "#FFF9E6",
                            3 or 4 => "#EDF7EE",
                            5 => "#F8F9FA",
                            _ => "#F0F4FF"
                        },
                        CardBorderColor = dto.Type switch
                        {
                            1 => "#C5D4F5",
                            2 => "#FFE082",
                            3 or 4 => "#C3E6C7",
                            5 => "#E4E7F4",
                            _ => "#C5D4F5"
                        },
                        StatusSummary = matchedController?.IsOnline == true
                            ? LocalizationService.Instance["Tank_OnlineNormal"]
                            : LocalizationService.Instance["Tank_OfflineRequired"],
                        StatusDotColor = matchedController?.IsOnline == true ? "#4CAF50" : "#FFA000",
                        IsNormalStatus = matchedController?.IsOnline ?? false
                    };

                    mappedList.Add(model);
                }

                lock (_syncLock)
                {
                    _aquariums.Clear();
                    _aquariums.AddRange(mappedList);
                }

                return mappedList;
            }
        }
        catch
        {
        }

        lock (_syncLock)
        {
            return _aquariums.ToList();
        }
    }

    public async Task<AquariumModel?> GetAquariumByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _apiClient.GetAsync<EcosystemDto>($"{ApiConfiguration.Routes.Ecosystems}/{id}", cancellationToken);
            if (response.IsSuccess && response.Data != null)
            {
                var dto = response.Data;
                lock (_syncLock)
                {
                    var existing = _aquariums.FirstOrDefault(x => x.Id == id);
                    if (existing != null)
                    {
                        existing.Name = dto.Name;
                        existing.VolumeLiters = dto.Volume ?? existing.VolumeLiters;
                        existing.ControllerId = dto.ControllerId;
                        return existing;
                    }
                }

                return new AquariumModel
                {
                    Id = dto.Id,
                    ControllerId = dto.ControllerId,
                    Name = dto.Name,
                    VolumeLiters = dto.Volume ?? 20.0,
                    SpeciesInfo = $"{LocalizationService.Instance["Tank_EcosystemPrefix"]}: {dto.Name}"
                };
            }
        }
        catch
        {
        }

        lock (_syncLock)
        {
            return _aquariums.FirstOrDefault(x => x.Id == id);
        }
    }

    public async Task<Guid> EnsureControllerForAquariumAsync(Guid aquariumId, CancellationToken cancellationToken = default)
    {
        AquariumModel? aq = null;
        lock (_syncLock)
        {
            aq = _aquariums.FirstOrDefault(a => a.Id == aquariumId);
        }

        if (aq != null && aq.ControllerId != Guid.Empty)
        {
            return aq.ControllerId;
        }

        // 1. Try to find an unassigned controller already associated with this account
        try
        {
            var response = await _apiClient.GetAsync<List<ControllerDto>>(ApiConfiguration.Routes.Controllers, cancellationToken);
            if (response.IsSuccess && response.Data != null && response.Data.Count > 0)
            {
                HashSet<Guid> assignedIds;
                lock (_syncLock)
                {
                    assignedIds = _aquariums
                        .Where(a => a.ControllerId != Guid.Empty && a.Id != aquariumId)
                        .Select(a => a.ControllerId)
                        .ToHashSet();
                }

                var matched = !string.IsNullOrWhiteSpace(aq?.ControllerName)
                    ? response.Data.FirstOrDefault(c => c.Name.Equals(aq.ControllerName, StringComparison.OrdinalIgnoreCase) && !assignedIds.Contains(c.Id))
                    : null;

                var controller = matched ?? response.Data.FirstOrDefault(c => !assignedIds.Contains(c.Id));
                if (controller != null)
                {
                    lock (_syncLock)
                    {
                        if (aq != null)
                        {
                            aq.ControllerId = controller.Id;
                            aq.ControllerName = controller.Name;
                        }
                    }
                    return controller.Id;
                }
            }
        }
        catch
        {
        }

        // 2. Register a new real controller with a valid 17-char MAC address matching regex ^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$
        try
        {
            var name = aq != null && !string.IsNullOrWhiteSpace(aq.Name)
                ? $"Контроллер {aq.Name}"
                : $"Контроллер Hub-{Random.Shared.Next(10, 99)}";

            var b = new byte[4];
            Random.Shared.NextBytes(b);
            var mac = $"02:00:{b[0]:X2}:{b[1]:X2}:{b[2]:X2}:{b[3]:X2}".ToUpperInvariant();
            var regRequest = new AddControllerRequestDto(mac, name, true);

            var regResponse = await _apiClient.PostAsync<AddControllerRequestDto, ControllerRegisteredResponse>(
                ApiConfiguration.Routes.Controllers, regRequest, cancellationToken);

            if (regResponse.IsSuccess && regResponse.Data != null && regResponse.Data.ControllerId != Guid.Empty)
            {
                var cid = regResponse.Data.ControllerId;
                lock (_syncLock)
                {
                    if (aq != null)
                    {
                        aq.ControllerId = cid;
                        aq.ControllerName = name;
                    }
                }
                return cid;
            }
        }
        catch
        {
        }

        var fallbackId = aq?.ControllerId != null && aq.ControllerId != Guid.Empty
            ? aq.ControllerId
            : Guid.NewGuid();

        lock (_syncLock)
        {
            if (aq != null)
            {
                aq.ControllerId = fallbackId;
            }
        }

        return fallbackId;
    }

    public async Task<AquariumModel> AddAquariumAsync(AquariumModel model, CancellationToken cancellationToken = default)
    {
        try
        {
            int ecosystemType = model.EcosystemType > 0 ? model.EcosystemType : 1;

            var controllerId = model.ControllerId != Guid.Empty
                ? model.ControllerId
                : await EnsureControllerForAquariumAsync(model.Id, cancellationToken);
            model.ControllerId = controllerId;

            var request = new CreateEcosystemRequestDto(
                ecosystemType,
                model.Name,
                model.VolumeLiters,
                controllerId);

            var response = await _apiClient.PostAsync<CreateEcosystemRequestDto, Guid>(
                ApiConfiguration.Routes.Ecosystems,
                request,
                cancellationToken);

            if (response.IsSuccess && response.Data != Guid.Empty)
            {
                model.Id = response.Data;
            }
        }
        catch
        {
        }

        if (model.Id == Guid.Empty)
        {
            model.Id = Guid.NewGuid();
        }

        lock (_syncLock)
        {
            _aquariums.Add(model);
        }
        return model;
    }

    public async Task<AquariumModel> UpdateAquariumAsync(AquariumModel model, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new UpdateEcosystemRequestDto(
                model.Name,
                model.VolumeLiters,
                Guid.Empty);

            await _apiClient.PutAsync<UpdateEcosystemRequestDto>(
                $"{ApiConfiguration.Routes.Ecosystems}/{model.Id}",
                request,
                cancellationToken);
        }
        catch
        {
        }

        lock (_syncLock)
        {
            var existing = _aquariums.FirstOrDefault(x => x.Id == model.Id);
            if (existing != null)
            {
                existing.Name = model.Name;
                existing.VolumeLiters = model.VolumeLiters;
                existing.WaterType = model.WaterType;
                existing.SpeciesInfo = model.SpeciesInfo;
                existing.Dimensions = model.Dimensions;
                existing.CardThemeColor = model.CardThemeColor;
                existing.CardBorderColor = model.CardBorderColor;
                existing.ControllerName = model.ControllerName;
                existing.TankTypeSubtitle = model.TankTypeSubtitle;
                existing.Icon = model.Icon;
            }
            else
            {
                _aquariums.Add(model);
            }
        }

        return model;
    }

    public async Task<bool> DeleteAquariumAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _apiClient.DeleteAsync($"{ApiConfiguration.Routes.Ecosystems}/{id}", cancellationToken);
        }
        catch
        {
        }

        lock (_syncLock)
        {
            _aquariums.RemoveAll(x => x.Id == id);
        }

        // Clean up cascaded data in sub-services
        _sensorService.RemoveSensorsForAquarium(id);
        _relayService.RemoveRelaysForAquarium(id);
        _scenarioService.RemoveScenariosForAquarium(id);
        _vacationModeService.RemoveVacationModesForAquarium(id);
        _maintenanceService.RemoveMaintenanceLogsForAquarium(id);
        _scheduleService.RemoveSchedulesForAquarium(id);

        return true;
    }

    #endregion

    #region Sensors (Delegated to ISensorService)

    public async Task<List<SensorModel>> GetSensorsAsync(Guid aquariumId, CancellationToken cancellationToken = default)
    {
        var list = await _sensorService.GetSensorsAsync(aquariumId, cancellationToken);
        lock (_syncLock)
        {
            var aq = _aquariums.FirstOrDefault(a => a.Id == aquariumId);
            if (aq != null) aq.SensorsCount = list.Count;
        }
        return list;
    }

    public async Task<SensorModel> AddSensorAsync(SensorModel sensor, CancellationToken cancellationToken = default)
    {
        if (sensor.ControllerId == Guid.Empty)
        {
            sensor.ControllerId = await EnsureControllerForAquariumAsync(sensor.AquariumId, cancellationToken);
        }

        var result = await _sensorService.AddSensorAsync(sensor, cancellationToken);
        lock (_syncLock)
        {
            var aq = _aquariums.FirstOrDefault(a => a.Id == sensor.AquariumId);
            if (aq != null) aq.SensorsCount = _sensorService.GetSensorsCount(sensor.AquariumId);
        }
        return result;
    }

    public async Task<bool> DeleteSensorAsync(Guid sensorId, CancellationToken cancellationToken = default)
    {
        var result = await _sensorService.DeleteSensorAsync(sensorId, cancellationToken);
        return result;
    }

    #endregion

    #region Relays (Delegated to IRelayService)

    public async Task<List<RelayModel>> GetRelaysAsync(Guid aquariumId, CancellationToken cancellationToken = default)
    {
        var list = await _relayService.GetRelaysAsync(aquariumId, cancellationToken);
        lock (_syncLock)
        {
            var aq = _aquariums.FirstOrDefault(a => a.Id == aquariumId);
            if (aq != null) aq.RelaysCount = list.Count;
        }
        return list;
    }

    public async Task<RelayModel> AddRelayAsync(RelayModel relay, CancellationToken cancellationToken = default)
    {
        if (relay.ControllerId == Guid.Empty)
        {
            relay.ControllerId = await EnsureControllerForAquariumAsync(relay.AquariumId, cancellationToken);
        }

        var result = await _relayService.AddRelayAsync(relay, cancellationToken);
        lock (_syncLock)
        {
            var aq = _aquariums.FirstOrDefault(a => a.Id == relay.AquariumId);
            if (aq != null) aq.RelaysCount = _relayService.GetRelaysCount(relay.AquariumId);
        }
        return result;
    }

    public Task<bool> ToggleRelayAsync(Guid relayId, CancellationToken cancellationToken = default) =>
        _relayService.ToggleRelayAsync(relayId, cancellationToken);

    public Task<bool> DeleteRelayAsync(Guid relayId, CancellationToken cancellationToken = default) =>
        _relayService.DeleteRelayAsync(relayId, cancellationToken);

    #endregion

    #region Scenarios (Delegated to IScenarioService)

    public async Task<List<ScenarioModel>> GetScenariosAsync(Guid aquariumId, CancellationToken cancellationToken = default)
    {
        var list = await _scenarioService.GetScenariosAsync(aquariumId, cancellationToken);
        lock (_syncLock)
        {
            var aq = _aquariums.FirstOrDefault(a => a.Id == aquariumId);
            if (aq != null) aq.ScenariosCount = list.Count;
        }
        return list;
    }

    public async Task<ScenarioModel> AddScenarioAsync(ScenarioModel scenario, CancellationToken cancellationToken = default)
    {
        var result = await _scenarioService.AddScenarioAsync(scenario, cancellationToken);
        lock (_syncLock)
        {
            var aq = _aquariums.FirstOrDefault(a => a.Id == scenario.AquariumId);
            if (aq != null) aq.ScenariosCount = _scenarioService.GetScenariosCount(scenario.AquariumId);
        }
        return result;
    }

    public Task<bool> ToggleScenarioAsync(Guid scenarioId, CancellationToken cancellationToken = default) =>
        _scenarioService.ToggleScenarioAsync(scenarioId, cancellationToken);

    public Task<bool> DeleteScenarioAsync(Guid scenarioId, CancellationToken cancellationToken = default) =>
        _scenarioService.DeleteScenarioAsync(scenarioId, cancellationToken);

    #endregion

    #region Vacation Modes (Delegated to IVacationModeService)

    public Task<List<VacationModeDto>> GetVacationModesAsync(Guid ecosystemId, CancellationToken cancellationToken = default) =>
        _vacationModeService.GetVacationModesAsync(ecosystemId, cancellationToken);

    public Task<bool> CreateVacationModeAsync(CreateVacationModeRequestDto dto, CancellationToken cancellationToken = default) =>
        _vacationModeService.CreateVacationModeAsync(dto, cancellationToken);

    public Task<bool> ToggleVacationModeAsync(Guid vacationModeId, CancellationToken cancellationToken = default) =>
        _vacationModeService.ToggleVacationModeAsync(vacationModeId, cancellationToken);

    public Task<bool> DeleteVacationModeAsync(Guid vacationModeId, CancellationToken cancellationToken = default) =>
        _vacationModeService.DeleteVacationModeAsync(vacationModeId, cancellationToken);

    #endregion

    #region Maintenance Logs (Delegated to IMaintenanceService)

    public Task<List<MaintenanceLogDto>> GetMaintenanceLogsAsync(Guid ecosystemId, CancellationToken cancellationToken = default) =>
        _maintenanceService.GetMaintenanceLogsAsync(ecosystemId, cancellationToken);

    public Task<bool> AddMaintenanceLogAsync(CreateMaintenanceLogRequestDto dto, CancellationToken cancellationToken = default) =>
        _maintenanceService.AddMaintenanceLogAsync(dto, cancellationToken);

    public Task<bool> DeleteMaintenanceLogAsync(Guid logId, CancellationToken cancellationToken = default) =>
        _maintenanceService.DeleteMaintenanceLogAsync(logId, cancellationToken);

    #endregion

    #region Schedules (Delegated to IScheduleService)

    public Task<List<ScheduleDto>> GetSchedulesAsync(Guid ecosystemId, CancellationToken cancellationToken = default) =>
        _scheduleService.GetSchedulesAsync(ecosystemId, cancellationToken);

    public Task<bool> CreateScheduleAsync(CreateScheduleRequestDto dto, CancellationToken cancellationToken = default) =>
        _scheduleService.CreateScheduleAsync(dto, cancellationToken);

    public Task<bool> ToggleScheduleAsync(Guid scheduleId, bool isActive, CancellationToken cancellationToken = default) =>
        _scheduleService.ToggleScheduleAsync(scheduleId, isActive, cancellationToken);

    public Task<bool> DeleteScheduleAsync(Guid scheduleId, CancellationToken cancellationToken = default) =>
        _scheduleService.DeleteScheduleAsync(scheduleId, cancellationToken);

    #endregion

    #region Reminders (Delegated to IReminderService)

    public Task<List<ReminderDto>> GetRemindersAsync(Guid? ecosystemId = null, CancellationToken cancellationToken = default) =>
        _reminderService.GetRemindersAsync(ecosystemId, cancellationToken);

    public Task<bool> CreateReminderAsync(CreateReminderRequestDto dto, CancellationToken cancellationToken = default) =>
        _reminderService.CreateReminderAsync(dto, cancellationToken);

    public Task<bool> CompleteReminderAsync(Guid reminderId, CancellationToken cancellationToken = default) =>
        _reminderService.CompleteReminderAsync(reminderId, cancellationToken);

    public Task<bool> DeleteReminderAsync(Guid reminderId, CancellationToken cancellationToken = default) =>
        _reminderService.DeleteReminderAsync(reminderId, cancellationToken);

    #endregion
}
