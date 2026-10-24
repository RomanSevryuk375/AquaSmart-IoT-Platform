using Mobile.Models;
using Mobile.Services.Dto.Control;
using Mobile.Services.Dto.Device;

namespace Mobile.Services;

public sealed class AquariumService : IAquariumService
{
    private readonly IApiClient _apiClient;
    private readonly object _syncLock = new();
    private readonly List<AquariumModel> _aquariums = new();
    private readonly List<SensorModel> _sensors = new();
    private readonly List<RelayModel> _relays = new();
    private readonly List<ScenarioModel> _scenarios = new();

    public AquariumService(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

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
                        Name = dto.Name,
                        VolumeLiters = dto.Volume ?? 20.0,
                        WaterType = (WaterType)(dto.Type switch
                        {
                            1 => 0, // Freshwater
                            2 => 1, // Marine
                            _ => 0
                        }),
                        Dimensions = "—",
                        SpeciesInfo = $"Экосистема: {dto.Name}",
                        ControllerName = matchedController != null ? $"Контроллер: {matchedController.Name}" : "Без контроллера",
                        IsOnline = matchedController?.IsOnline ?? false,
                        Temperature = tempVal ?? 0.0,
                        TemperatureDisplay = tempVal.HasValue ? $"{tempVal.Value:F1}°C" : "-- °C",
                        SensorsCount = matchedSensors.Count,
                        RelaysCount = matchedRelays.Count,
                        ScenariosCount = 0,
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
                            1 => $"Аквариум • {dto.Volume ?? 20:F0}л",
                            2 => $"Террариум • {dto.Volume ?? 30:F0}л",
                            3 => $"Гроубокс • {dto.Volume ?? 15:F0}л",
                            4 => $"Флорариум • {dto.Volume ?? 5.5:F1}л",
                            5 => $"Формикарий • {dto.Volume ?? 2:F0}л",
                            _ => $"Аквариум • {dto.Volume ?? 20:F0}л"
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
                            ? "• В сети • Всё в порядке"
                            : "• Не в сети • Требуется подключение",
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
                        return existing;
                    }
                }

                return new AquariumModel
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    VolumeLiters = dto.Volume ?? 20.0,
                    SpeciesInfo = $"Экосистема: {dto.Name}"
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

    public async Task<AquariumModel> AddAquariumAsync(AquariumModel model, CancellationToken cancellationToken = default)
    {
        try
        {
            int ecosystemType = model.WaterType switch
            {
                WaterType.Marine => 2,
                _ => 1
            };

            var request = new CreateEcosystemRequestDto(
                ecosystemType,
                model.Name,
                model.VolumeLiters,
                Guid.NewGuid());

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

        lock (_syncLock)
        {
            _aquariums.Add(model);
        }
        return model;
    }

    public async Task<List<SensorModel>> GetSensorsAsync(Guid aquariumId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _apiClient.GetAsync<List<SensorDto>>(ApiConfiguration.Routes.Sensors, cancellationToken);
            if (response.IsSuccess && response.Data != null)
            {
                var backendSensors = response.Data.Select(dto => new SensorModel
                {
                    Id = dto.Id,
                    AquariumId = aquariumId,
                    Name = dto.Name,
                    Value = dto.LastValue,
                    Unit = string.IsNullOrWhiteSpace(dto.Unit) ? "°C" : dto.Unit,
                    Port = dto.ConnectionAddress ?? string.Empty,
                    Type = dto.Type switch
                    {
                        1 => SensorType.Temperature,
                        2 => SensorType.WaterLevel,
                        3 => SensorType.Ph,
                        4 => SensorType.DissolvedOxygen,
                        _ => SensorType.Temperature
                    },
                    MinThreshold = 20.0,
                    MaxThreshold = 30.0,
                    LastUpdated = dto.UpdatedAt != default ? dto.UpdatedAt : DateTime.Now
                }).ToList();

                lock (_syncLock)
                {
                    _sensors.Clear();
                    _sensors.AddRange(backendSensors);
                }

                return backendSensors;
            }
        }
        catch
        {
        }

        lock (_syncLock)
        {
            return _sensors.Where(x => x.AquariumId == aquariumId).ToList();
        }
    }

    public async Task<SensorModel> AddSensorAsync(SensorModel sensor, CancellationToken cancellationToken = default)
    {
        try
        {
            var rawPort = string.IsNullOrWhiteSpace(sensor.Port) ? "GPIO 4" : sensor.Port.Trim();
            var cleanPort = rawPort.Replace(" ", "");
            int protocol = 1; // Digital (e.g. GPIO4, D4, 4)
            if (cleanPort.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                protocol = 3; // I2C
            }
            else if (cleanPort.StartsWith("A", StringComparison.OrdinalIgnoreCase) || cleanPort.StartsWith("ADC", StringComparison.OrdinalIgnoreCase))
            {
                protocol = 2; // Analog
            }

            var request = new AddSensorRequestDto(
                Guid.NewGuid(),
                sensor.Name,
                protocol,
                cleanPort,
                (int)sensor.Type + 1);

            await _apiClient.PostAsync(ApiConfiguration.Routes.Sensors, request, cancellationToken);
        }
        catch
        {
        }

        lock (_syncLock)
        {
            _sensors.Add(sensor);
        }
        return sensor;
    }

    public async Task<List<RelayModel>> GetRelaysAsync(Guid aquariumId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _apiClient.GetAsync<List<RelayDto>>(ApiConfiguration.Routes.Relays, cancellationToken);
            if (response.IsSuccess && response.Data != null)
            {
                var backendRelays = response.Data.Select((dto, index) => new RelayModel
                {
                    Id = dto.Id,
                    AquariumId = aquariumId,
                    Name = dto.Name,
                    IsOn = dto.IsActive,
                    PinChannel = index + 1,
                    Type = dto.Purpose switch
                    {
                        0 => AquariumDeviceType.FilterPump,
                        1 => AquariumDeviceType.Heater,
                        2 => AquariumDeviceType.Lighting,
                        3 => AquariumDeviceType.Aerator,
                        4 => AquariumDeviceType.Heater,
                        _ => AquariumDeviceType.Aerator
                    },
                    ScheduleInfo = dto.IsManual ? "Ручной режим" : "По расписанию"
                }).ToList();

                lock (_syncLock)
                {
                    _relays.Clear();
                    _relays.AddRange(backendRelays);
                }

                return backendRelays;
            }
        }
        catch
        {
        }

        lock (_syncLock)
        {
            return _relays.Where(x => x.AquariumId == aquariumId).ToList();
        }
    }

    public async Task<RelayModel> AddRelayAsync(RelayModel relay, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new AddRelayRequestDto(
                Guid.NewGuid(),
                null,
                relay.Name,
                4, // GPIO
                $"GPIO{relay.PinChannel}",
                true,
                (int)relay.Type,
                relay.IsOn,
                true);

            await _apiClient.PostAsync(ApiConfiguration.Routes.Relays, request, cancellationToken);
        }
        catch
        {
        }

        lock (_syncLock)
        {
            _relays.Add(relay);
        }
        return relay;
    }

    public async Task<bool> ToggleRelayAsync(Guid relayId, CancellationToken cancellationToken = default)
    {
        try
        {
            var route = $"{ApiConfiguration.Routes.Commands}/toggle-state/{relayId}";
            await _apiClient.PostAsync(route, new { }, cancellationToken);
        }
        catch
        {
        }

        lock (_syncLock)
        {
            var relay = _relays.FirstOrDefault(x => x.Id == relayId);
            if (relay != null)
            {
                relay.IsOn = !relay.IsOn;
                return true;
            }
        }

        return true;
    }

    public async Task<List<ScenarioModel>> GetScenariosAsync(Guid aquariumId, CancellationToken cancellationToken = default)
    {
        try
        {
            var route = $"{ApiConfiguration.Routes.AutomationRules}?ecosystemId={aquariumId}";
            var response = await _apiClient.GetAsync<List<AutomationRuleDto>>(route, cancellationToken);
            if (response.IsSuccess && response.Data != null)
            {
                var mapped = response.Data.Select(dto => new ScenarioModel
                {
                    Id = dto.Id,
                    AquariumId = dto.EcosystemId,
                    Name = dto.Name,
                    IsActive = dto.IsActive,
                    ConditionText = dto.Conditions?.Count > 0
                        ? $"Датчик: условие порога > {dto.Conditions[0].Threshold}"
                        : "По расписанию",
                    ActionText = dto.Action == 0 ? "Включить реле" : "Выключить реле",
                    FrequencyText = "Автоматически"
                }).ToList();

                lock (_syncLock)
                {
                    _scenarios.Clear();
                    _scenarios.AddRange(mapped);
                }

                return mapped;
            }
        }
        catch
        {
        }

        lock (_syncLock)
        {
            return _scenarios.Where(x => x.AquariumId == aquariumId).ToList();
        }
    }

    public Task<ScenarioModel> AddScenarioAsync(ScenarioModel scenario, CancellationToken cancellationToken = default)
    {
        lock (_syncLock)
        {
            _scenarios.Add(scenario);
        }
        return Task.FromResult(scenario);
    }

    public Task<bool> ToggleScenarioAsync(Guid scenarioId, CancellationToken cancellationToken = default)
    {
        lock (_syncLock)
        {
            var sc = _scenarios.FirstOrDefault(x => x.Id == scenarioId);
            if (sc != null)
            {
                sc.IsActive = !sc.IsActive;
                return Task.FromResult(true);
            }
        }
        return Task.FromResult(false);
    }
}
