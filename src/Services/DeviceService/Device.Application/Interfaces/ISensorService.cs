using Contracts.Enums;
using Device.Application.DTOs.Sensor;

namespace Device.Application.Interfaces;

public interface ISensorService
{
    Task<IReadOnlyList<SensorResponseDto>> GetAllSensorsAsync(
        SensorFilterDto filter,
        int? skip,
        int? take,
        CancellationToken cancellationToken);

    Task<SensorResponseDto> GetSensorByIdAsync(
        Guid sensorId, 
        CancellationToken cancellationToken);

    Task<Guid> AddSensorAsync(
        SensorRequestDto request, 
        CancellationToken cancellationToken);

    Task UpdateSensorAsync(
        Guid sensorId,
        SensorUpdateRequestDto updateRequestDto,
        CancellationToken cancellationToken);

    Task DeleteSensorAsync(
        Guid sensorId, 
        CancellationToken cancellationToken);

    Task SetSensorStateAsync(
        Guid sensorId,
        SensorStateEnum state,
        CancellationToken cancellationToken);
}
