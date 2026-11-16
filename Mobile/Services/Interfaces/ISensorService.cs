using Mobile.Models;

namespace Mobile.Services.Interfaces;

public interface ISensorService
{
    Task<List<SensorModel>> GetSensorsAsync(Guid aquariumId, CancellationToken cancellationToken = default);
    Task<SensorModel> AddSensorAsync(SensorModel sensor, CancellationToken cancellationToken = default);
    Task<bool> DeleteSensorAsync(Guid sensorId, CancellationToken cancellationToken = default);
    void RemoveSensorsForAquarium(Guid aquariumId);
    int GetSensorsCount(Guid aquariumId);
}
