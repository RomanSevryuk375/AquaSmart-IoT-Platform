using Device.Application.DTOs.Relay;

namespace Device.Application.Interfaces
{
    public interface IRelayService
    {
        Task<Guid> AddRelayAsync(
            RelayRequestDto request, 
            CancellationToken cancellationToken);

        Task DeleteRelayAsync(
            Guid relayId, 
            CancellationToken cancellationToken);

        Task<IReadOnlyList<RelayResponseDto>> GetAllRelaysAsync(
            RelayFilterDto filter, 
            int? skip, 
            int? take, 
            CancellationToken cancellationToken);

        Task<RelayResponseDto> GetRelayByIdAsync(
            Guid relayId, 
            CancellationToken cancellationToken);

        Task SetRelayPowerSensorAsync(
            Guid relayId, 
            Guid powerSensorId, 
            CancellationToken cancellationToken);

        Task UpdateRelayAsync(
            Guid relayId, 
            RelayUpdateRequestDto updateRequestDto, 
            CancellationToken cancellationToken);
    }
}