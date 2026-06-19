namespace Device.Application.Interfaces
{
    public interface IRelayService
    {
        Task<Result<RelayResponseDto>> AddRelayAsync(
            RelayRequestDto request, 
            CancellationToken cancellationToken);

        Task<Result> DeleteRelayAsync(
            Guid relayId, 
            CancellationToken cancellationToken);

        Task<Result<IReadOnlyList<RelayResponseDto>>> GetAllRelaysAsync(
            RelayFilterDto filter, 
            int? skip, 
            int? take, 
            CancellationToken cancellationToken);

        Task<Result<RelayResponseDto>> GetRelayByIdAsync(
            Guid relayId, 
            CancellationToken cancellationToken);

        Task<Result> SetRelayPowerSensorAsync(
            Guid relayId, 
            Guid powerSensorId, 
            CancellationToken cancellationToken);

        Task<Result> UpdateRelayAsync(
            Guid relayId, 
            RelayUpdateRequestDto updateRequestDto, 
            CancellationToken cancellationToken);
    }
}