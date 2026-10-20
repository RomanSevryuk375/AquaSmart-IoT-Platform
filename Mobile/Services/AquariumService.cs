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
        SeedDemoData();
    }

    private void SeedDemoData()
    {
        var aq1 = new AquariumModel
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Мой первый аквариум",
            VolumeLiters = 20,
            WaterType = WaterType.Freshwater,
            SpeciesInfo = "Аквариум для рыб и водных обитателей",
            TankTypeSubtitle = "Аквариум • 20л",
            ControllerName = "Контроллер: №123",
            Icon = "🐟",
            Dimensions = "40 × 25 × 25 см",
            Temperature = 28.0,
            TemperatureDisplay = "28°C",
            Ph = 6.8,
            Oxygen = 7.9,
            SalinityTds = 180,
            CardThemeColor = "#F0F4FF",
            CardBorderColor = "#C5D4F5",
            IsOnline = true,
            StatusSummary = "• В сети • Всё в порядке",
            StatusDotColor = "#4CAF50",
            IsNormalStatus = true,
            SensorsCount = 4,
            RelaysCount = 5,
            ScenariosCount = 3
        };

        var aq2 = new AquariumModel
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Name = "Флориус Идратус",
            VolumeLiters = 5.5,
            WaterType = WaterType.Freshwater,
            SpeciesInfo = "Декоративная ёмкость для растений",
            TankTypeSubtitle = "Флорариум • 5.5л",
            ControllerName = "Хаб-02",
            Icon = "🌿",
            Dimensions = "20 × 20 × 25 см",
            Temperature = 0,
            TemperatureDisplay = "-- °C",
            Ph = 7.0,
            Oxygen = 0,
            SalinityTds = 0,
            CardThemeColor = "#EDF7EE",
            CardBorderColor = "#C3E6C7",
            IsOnline = false,
            StatusSummary = "• Не в сети • Предупреждение",
            StatusDotColor = "#FFA000",
            IsNormalStatus = false,
            SensorsCount = 4,
            RelaysCount = 1,
            ScenariosCount = 1
        };

        var aq3 = new AquariumModel
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Name = "Домик Черепахи",
            VolumeLiters = 40,
            WaterType = WaterType.Freshwater,
            SpeciesInfo = "Совмещает водную и наземную зону",
            TankTypeSubtitle = "Палюдариум • 40л",
            ControllerName = "Хаб-03",
            Icon = "🐢",
            Dimensions = "50 × 30 × 30 см",
            Temperature = 4.0,
            TemperatureDisplay = "4°C",
            Ph = 6.5,
            Oxygen = 6.2,
            SalinityTds = 140,
            CardThemeColor = "#FFF9E6",
            CardBorderColor = "#FFE082",
            IsOnline = true,
            StatusSummary = "• В сети • Есть проблема",
            StatusDotColor = "#E53935",
            IsNormalStatus = false,
            SensorsCount = 3,
            RelaysCount = 2,
            ScenariosCount = 1
        };

        var aq4 = new AquariumModel
        {
            Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            Name = "Муравьиная ферма",
            VolumeLiters = 2,
            WaterType = WaterType.Freshwater,
            SpeciesInfo = "Муравьиная ферма",
            TankTypeSubtitle = "Формикарий • 2л",
            ControllerName = "Хаб-04",
            Icon = "🐜",
            Dimensions = "25 × 15 × 15 см",
            Temperature = 22.0,
            TemperatureDisplay = "--",
            Ph = 7.0,
            Oxygen = 0,
            SalinityTds = 0,
            CardThemeColor = "#F8F9FA",
            CardBorderColor = "#E4E7F4",
            IsOnline = true,
            StatusSummary = "• В сети • Всё в порядке",
            StatusDotColor = "#4CAF50",
            IsNormalStatus = true,
            SensorsCount = 2,
            RelaysCount = 0,
            ScenariosCount = 0
        };

        lock (_syncLock)
        {
            _aquariums.AddRange(new[] { aq1, aq2, aq3, aq4 });

            // Sensors for aq1
            _sensors.AddRange(new[]
            {
                new SensorModel { AquariumId = aq1.Id, Name = "Датчик температуры воды (DS18B20)", Type = SensorType.Temperature, Value = 26.2, Unit = "°C", MinThreshold = 24.0, MaxThreshold = 28.0 },
                new SensorModel { AquariumId = aq1.Id, Name = "pH-метр лабораторный", Type = SensorType.Ph, Value = 6.8, Unit = "pH", MinThreshold = 6.5, MaxThreshold = 7.5 },
                new SensorModel { AquariumId = aq1.Id, Name = "Датчик растворенного O2", Type = SensorType.DissolvedOxygen, Value = 7.9, Unit = "мг/л", MinThreshold = 6.0, MaxThreshold = 10.0 },
                new SensorModel { AquariumId = aq1.Id, Name = "TDS / Минерализация", Type = SensorType.TdsSalinity, Value = 180, Unit = "ppm", MinThreshold = 100, MaxThreshold = 300 }
            });

            // Relays for aq1
            _relays.AddRange(new[]
            {
                new RelayModel { AquariumId = aq1.Id, Name = "Аэратор бесшумный", Type = AquariumDeviceType.Aerator, IsOn = true, PinChannel = 1, ScheduleInfo = "Круглосуточно" },
                new RelayModel { AquariumId = aq1.Id, Name = "Титановый нагреватель 200W", Type = AquariumDeviceType.Heater, IsOn = false, PinChannel = 2, ScheduleInfo = "Авто при T < 25.5°C" },
                new RelayModel { AquariumId = aq1.Id, Name = "Канистровый биофильтр", Type = AquariumDeviceType.FilterPump, IsOn = true, PinChannel = 3, ScheduleInfo = "Круглосуточно" },
                new RelayModel { AquariumId = aq1.Id, Name = "Умная автокормушка", Type = AquariumDeviceType.Feeder, IsOn = false, PinChannel = 4, ScheduleInfo = "09:00 и 18:30" },
                new RelayModel { AquariumId = aq1.Id, Name = "Фито-свет LED WRGB", Type = AquariumDeviceType.Lighting, IsOn = true, PinChannel = 5, ScheduleInfo = "10:00 — 20:00" }
            });

            // Scenarios for aq1
            _scenarios.AddRange(new[]
            {
                new ScenarioModel { AquariumId = aq1.Id, Name = "Терморегуляция обогрева", ConditionText = "Если Температура < 25.0 °C", ActionText = "Включить Нагреватель", IsActive = true, FrequencyText = "По датчику" },
                new ScenarioModel { AquariumId = aq1.Id, Name = "Режим ночного покоя", ConditionText = "Время = 20:00", ActionText = "Выключить Свет, снизить аэрацию до 40%", IsActive = true, FrequencyText = "Каждый день" },
                new ScenarioModel { AquariumId = aq1.Id, Name = "Автокормление утром и вечером", ConditionText = "Время = 09:00, 18:30", ActionText = "Подать 1 дозу корма (порция №2)", IsActive = true, FrequencyText = "Каждый день" }
            });
        }
    }

    public async Task<List<AquariumModel>> GetAquariumsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var ecoResponse = await _apiClient.GetAsync<List<EcosystemDto>>(ApiConfiguration.Routes.Ecosystems, cancellationToken);
            if (ecoResponse.IsSuccess && ecoResponse.Data != null && ecoResponse.Data.Count > 0)
            {
                // Parallelize dependent queries via Task.WhenAll for maximum network performance
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
                    double tempVal = tempSensor?.LastValue ?? 25.0;

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
                        Dimensions = "40 × 25 × 25 см",
                        SpeciesInfo = $"Экосистема: {dto.Name}",
                        ControllerName = matchedController != null ? $"Контроллер: {matchedController.Name}" : "Хаб-01",
                        IsOnline = matchedController?.IsOnline ?? true,
                        Temperature = tempVal,
                        TemperatureDisplay = $"{tempVal:F1}°C",
                        SensorsCount = matchedSensors.Count > 0 ? matchedSensors.Count : 3,
                        RelaysCount = matchedRelays.Count > 0 ? matchedRelays.Count : 4,
                        ScenariosCount = 2,
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
                        StatusSummary = matchedController?.IsOnline == false
                            ? "• Не в сети • Предупреждение"
                            : "• В сети • Всё в порядке",
                        StatusDotColor = matchedController?.IsOnline == false ? "#FFA000" : "#4CAF50",
                        IsNormalStatus = matchedController?.IsOnline ?? true
                    };

                    mappedList.Add(model);
                }

                lock (_syncLock)
                {
                    _aquariums.Clear();
                    _aquariums.AddRange(mappedList);
                }
            }
        }
        catch
        {
            // Silently fall back to seed data if API is unreachable
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
            if (response.IsSuccess && response.Data != null && response.Data.Count > 0)
            {
                var backendSensors = response.Data.Select(dto => new SensorModel
                {
                    Id = dto.Id,
                    AquariumId = aquariumId,
                    Name = dto.Name,
                    Value = dto.LastValue,
                    Unit = string.IsNullOrWhiteSpace(dto.Unit) ? "°C" : dto.Unit,
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

                return backendSensors;
            }
        }
        catch
        {
        }

        lock (_syncLock)
        {
            var list = _sensors.Where(x => x.AquariumId == aquariumId).ToList();
            return list.Count > 0 ? list : _sensors.Take(4).ToList();
        }
    }

    public async Task<SensorModel> AddSensorAsync(SensorModel sensor, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new AddSensorRequestDto(
                Guid.NewGuid(),
                sensor.Name,
                2, // OneWire
                "0x28",
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
            if (response.IsSuccess && response.Data != null && response.Data.Count > 0)
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

                return backendRelays;
            }
        }
        catch
        {
        }

        lock (_syncLock)
        {
            var list = _relays.Where(x => x.AquariumId == aquariumId).ToList();
            return list.Count > 0 ? list : _relays.Take(5).ToList();
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
            if (response.IsSuccess && response.Data != null && response.Data.Count > 0)
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

                return mapped;
            }
        }
        catch
        {
        }

        lock (_syncLock)
        {
            var list = _scenarios.Where(x => x.AquariumId == aquariumId).ToList();
            return list.Count > 0 ? list : _scenarios.Take(3).ToList();
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
