namespace Device.Application.Interfaces;

public interface IControllerService
{
    Task<Result<IReadOnlyList<ControllerResponseDto>>> GetAllControllersAsync(
        ControllerFilterDto filter,
        int? skip,
        int? take,
        CancellationToken cancellationToken);

    Task<Result<ControllerResponseDto>> GetControllerByIdAsync(
        Guid controllerId,
        CancellationToken cancellationToken);

    Task<Result<ControllerRegistredResponseDto>> AddControllerAsync(
        ControllerRequestDto request, 
        CancellationToken cancellationToken);

    Task<Result> UpdateControllerAsync(
        Guid controllerId,
        ControllerUpdateRequestDto updateRequestDto,
        CancellationToken cancellationToken);

    Task<Result> DeleteControllerAsync(
        Guid controllerId, 
        CancellationToken cancellationToken);

    Task<Result<ControllerPingResponseDto>> PingControllerAsync(
        Guid controllerId,
        string deviceToken,
        CancellationToken cancellationToken);

    Task<Result<bool>> ToggleControllerStateAsync(
        Guid controllerId, 
        CancellationToken cancellationToken);
}
