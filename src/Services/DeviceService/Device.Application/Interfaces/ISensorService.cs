using Contracts.Enums;
using Contracts.Results;
using Device.Application.DTOs.Sensor;

namespace Device.Application.Interfaces;

public interface ISensorService
{
    Task<Result<IReadOnlyList<SensorResponseDto>>> GetAllSensorsAsync(
        SensorFilterDto filter,
        int? skip,
        int? take,
        CancellationToken cancellationToken);

    Task<Result<SensorResponseDto>> GetSensorByIdAsync(
        Guid sensorId, 
        CancellationToken cancellationToken);

    Task<Result<Guid>> AddSensorAsync(
        SensorRequestDto request, 
        CancellationToken cancellationToken);

    Task<Result> UpdateSensorAsync(
        Guid sensorId,
        SensorUpdateRequestDto updateRequestDto,
        CancellationToken cancellationToken);

    Task<Result> DeleteSensorAsync(
        Guid sensorId, 
        CancellationToken cancellationToken);

    Task<Result> SetSensorStateAsync(
        Guid sensorId,
        SensorStateEnum state,
        CancellationToken cancellationToken);
}
