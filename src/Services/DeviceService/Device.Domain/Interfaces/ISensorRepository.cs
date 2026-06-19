namespace Device.Domain.Interfaces;

public interface ISensorRepository : IRepository<Sensor>
{
    public Task<bool> ExistsAsync(
        Guid sensorId, 
        CancellationToken cancellationToken = default);
    public Task<IReadOnlyList<Sensor>> GetAllSensorsAsync(
        Guid controllerId,
        CancellationToken cancellationToken = default);
}
