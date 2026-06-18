using Contracts.Abstractions;
using Device.Domain.Entities.Sensors;

namespace Device.Domain.Interfaces;

public interface ISensorRepository : IRepository<Sensor>
{
    Task<bool> ExistsAsync(
        Guid sensorId, 
        CancellationToken cancellationToken);
    Task<IReadOnlyList<Sensor>> GetAllSensorsAsync(
        Guid controllerId,
        CancellationToken cancellationToken);
}
