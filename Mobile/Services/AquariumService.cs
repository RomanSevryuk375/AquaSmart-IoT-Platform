using Mobile.Models;

namespace Mobile.Services;

public interface IAquariumService
{
    Task<List<AquariumModel>> GetAquariumsAsync();
    Task<AquariumModel?> GetAquariumByIdAsync(Guid id);
    Task<AquariumModel> AddAquariumAsync(AquariumModel model);
    Task<List<SensorModel>> GetSensorsAsync(Guid aquariumId);
    Task<SensorModel> AddSensorAsync(SensorModel sensor);
    Task<List<RelayModel>> GetRelaysAsync(Guid aquariumId);
    Task<RelayModel> AddRelayAsync(RelayModel relay);
    Task<bool> ToggleRelayAsync(Guid relayId);
    Task<List<ScenarioModel>> GetScenariosAsync(Guid aquariumId);
    Task<ScenarioModel> AddScenarioAsync(ScenarioModel scenario);
    Task<bool> ToggleScenarioAsync(Guid scenarioId);
}

public class AquariumService : IAquariumService
{
    private readonly List<AquariumModel> _aquariums = new();
    private readonly List<SensorModel> _sensors = new();
    private readonly List<RelayModel> _relays = new();
    private readonly List<ScenarioModel> _scenarios = new();

    public AquariumService()
    {
        SeedDemoData();
    }

    private void SeedDemoData()
    {
        var aq1 = new AquariumModel
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Основной пресноводный аквариум",
            VolumeLiters = 240,
            WaterType = WaterType.Freshwater,
            SpeciesInfo = "Дискусы, Неоны, Сомики коридорасы",
            Dimensions = "120 × 40 × 50 см",
            Temperature = 26.2,
            Ph = 6.8,
            Oxygen = 7.9,
            SalinityTds = 180,
            IsNormalStatus = true
        };

        var aq2 = new AquariumModel
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Name = "Морской рифовый резервуар",
            VolumeLiters = 450,
            WaterType = WaterType.Marine,
            SpeciesInfo = "Рыбы-клоуны, Актинии, Кораллы LPS/SPS",
            Dimensions = "150 × 60 × 60 см",
            Temperature = 25.0,
            Ph = 8.2,
            Oxygen = 8.4,
            SalinityTds = 35000,
            IsNormalStatus = true
        };

        var aq3 = new AquariumModel
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Name = "Мальковый карантинный бассейн",
            VolumeLiters = 80,
            WaterType = WaterType.Freshwater,
            SpeciesInfo = "Мальки скалярий",
            Dimensions = "60 × 35 × 40 см",
            Temperature = 28.1,
            Ph = 7.0,
            Oxygen = 6.9,
            SalinityTds = 150,
            IsNormalStatus = true
        };

        _aquariums.AddRange(new[] { aq1, aq2, aq3 });

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

    public Task<List<AquariumModel>> GetAquariumsAsync()
    {
        return Task.FromResult(_aquariums.ToList());
    }

    public Task<AquariumModel?> GetAquariumByIdAsync(Guid id)
    {
        var item = _aquariums.FirstOrDefault(x => x.Id == id);
        return Task.FromResult(item);
    }

    public Task<AquariumModel> AddAquariumAsync(AquariumModel model)
    {
        _aquariums.Add(model);
        return Task.FromResult(model);
    }

    public Task<List<SensorModel>> GetSensorsAsync(Guid aquariumId)
    {
        var list = _sensors.Where(x => x.AquariumId == aquariumId).ToList();
        return Task.FromResult(list);
    }

    public Task<SensorModel> AddSensorAsync(SensorModel sensor)
    {
        _sensors.Add(sensor);
        return Task.FromResult(sensor);
    }

    public Task<List<RelayModel>> GetRelaysAsync(Guid aquariumId)
    {
        var list = _relays.Where(x => x.AquariumId == aquariumId).ToList();
        return Task.FromResult(list);
    }

    public Task<RelayModel> AddRelayAsync(RelayModel relay)
    {
        _relays.Add(relay);
        return Task.FromResult(relay);
    }

    public Task<bool> ToggleRelayAsync(Guid relayId)
    {
        var relay = _relays.FirstOrDefault(x => x.Id == relayId);
        if (relay != null)
        {
            relay.IsOn = !relay.IsOn;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<List<ScenarioModel>> GetScenariosAsync(Guid aquariumId)
    {
        var list = _scenarios.Where(x => x.AquariumId == aquariumId).ToList();
        return Task.FromResult(list);
    }

    public Task<ScenarioModel> AddScenarioAsync(ScenarioModel scenario)
    {
        _scenarios.Add(scenario);
        return Task.FromResult(scenario);
    }

    public Task<bool> ToggleScenarioAsync(Guid scenarioId)
    {
        var sc = _scenarios.FirstOrDefault(x => x.Id == scenarioId);
        if (sc != null)
        {
            sc.IsActive = !sc.IsActive;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
}
