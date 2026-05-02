using Device.Application.DTOs.Controller;

namespace Device.Application.Interfaces;

public interface IControllerService
{
    Task<IReadOnlyList<ControllerResponseDto>> GetAllControllersAsync(
        ControllerFilterDto filter,
        int? skip,
        int? take,
        CancellationToken cancellationToken);

    Task<ControllerResponseDto> GetControllerByIdAsync(
        Guid controllerId,
        CancellationToken cancellationToken);

    Task<ControllerRegistredResponseDto> AddControllerAsync(
        ControllerRequestDto request, 
        CancellationToken cancellationToken);

    Task UpdateControllerAsync(
        Guid controllerId,
        ControllerUpdateRequestDto updateRequestDto,
        CancellationToken cancellationToken);

    Task DeleteControllerAsync(
        Guid controllerId, 
        CancellationToken cancellationToken);

    Task<ControllerPingResponseDto> PingControllerAsync(
        Guid controllerId,
        string deviceToken,
        CancellationToken cancellationToken);

    Task<bool> ToggleControllerStateAsync(
        Guid controllerId, 
        CancellationToken cancellationToken);
}
