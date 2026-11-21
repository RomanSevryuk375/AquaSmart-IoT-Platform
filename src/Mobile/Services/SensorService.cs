using Mobile.Models;
using Mobile.Services.Dto.Device;
using Mobile.Services.Interfaces;

namespace Mobile.Services;

public sealed class SensorService : ISensorService
{
    private readonly IApiClient _apiClient;
    private readonly object _syncLock = new();
    private readonly List<SensorModel> _sensors = new();

    public SensorService(IApiClient apiClient)
    {
        _apiClient = apiClient;
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
                    ControllerId = dto.ControllerId,
                    Name = dto.Name,
                    Value = dto.LastValue,
                    Unit = string.IsNullOrWhiteSpace(dto.Unit) ? "°C" : dto.Unit,
                    Port = dto.ConnectionAddress ?? string.Empty,
                    Type = dto.Type switch
                    {
                        1 => SensorType.Temperature,
                        2 => SensorType.Ph,
                        3 => SensorType.DissolvedOxygen,
                        4 => SensorType.TdsSalinity,
                        _ => SensorType.Temperature
                    },
                    MinThreshold = 20.0,
                    MaxThreshold = 30.0,
                    LastUpdated = dto.UpdatedAt != default ? dto.UpdatedAt : DateTime.Now
                }).ToList();

                lock (_syncLock)
                {
                    var localOnly = _sensors
                        .Where(s => s.AquariumId == aquariumId && !backendSensors.Any(b => b.Id == s.Id))
                        .ToList();

                    _sensors.RemoveAll(s => s.AquariumId == aquariumId);
                    _sensors.AddRange(backendSensors);
                    _sensors.AddRange(localOnly);

                    return _sensors.Where(x => x.AquariumId == aquariumId).ToList();
                }
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
            int protocol = 1;
            if (cleanPort.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                protocol = 3;
            }
            else if (cleanPort.StartsWith("A", StringComparison.OrdinalIgnoreCase) || cleanPort.StartsWith("ADC", StringComparison.OrdinalIgnoreCase))
            {
                protocol = 2;
            }

            int backendSensorType = sensor.Type switch
            {
                SensorType.Temperature => 1,
                SensorType.WaterLevel or SensorType.Ph => 2,
                SensorType.DissolvedOxygen => 3,
                SensorType.TdsSalinity => 4,
                _ => 1
            };

            var request = new AddSensorRequestDto(
                sensor.ControllerId,
                sensor.Name,
                protocol,
                cleanPort,
                backendSensorType);

            var response = await _apiClient.PostAsync<AddSensorRequestDto, SensorCreatedResponse>(
                ApiConfiguration.Routes.Sensors, request, cancellationToken);

            if (response.IsSuccess && response.Data != null && response.Data.Id != Guid.Empty)
            {
                sensor.Id = response.Data.Id;
            }
        }
        catch
        {
        }

        if (sensor.Id == Guid.Empty)
        {
            sensor.Id = Guid.NewGuid();
        }

        lock (_syncLock)
        {
            var existingIdx = _sensors.FindIndex(s => s.Id == sensor.Id);
            if (existingIdx >= 0)
            {
                _sensors[existingIdx] = sensor;
            }
            else
            {
                _sensors.Add(sensor);
            }
        }

        return sensor;
    }

    public async Task<bool> DeleteSensorAsync(Guid sensorId, CancellationToken cancellationToken = default)
    {
        try
        {
            var route = $"{ApiConfiguration.Routes.Sensors}/{sensorId}";
            await _apiClient.DeleteAsync(route, cancellationToken);
        }
        catch
        {
        }

        lock (_syncLock)
        {
            _sensors.RemoveAll(s => s.Id == sensorId);
        }

        return true;
    }

    public void RemoveSensorsForAquarium(Guid aquariumId)
    {
        lock (_syncLock)
        {
            _sensors.RemoveAll(s => s.AquariumId == aquariumId);
        }
    }

    public int GetSensorsCount(Guid aquariumId)
    {
        lock (_syncLock)
        {
            return _sensors.Count(s => s.AquariumId == aquariumId);
        }
    }
}
