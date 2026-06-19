using Contracts.Events.RelayEvents;
using Device.Application.DTOs.RelayCommands;

namespace Device.Application.Interfaces;

public interface IRelayCommandQueueService
{
    Task<Result<IReadOnlyList<RelayCommandResponseDto>>> GetPendingCommands(
        Guid controllerId,
        string deviceToken,
        CancellationToken cancellationToken);

    Task<Result> MarkAsCompletedByIdAsync(
        Guid commandId,
        string deviceToken,
        CancellationToken cancellationToken);

    Task<Result> MarkAsFailedByIdAsync(
        Guid commandId,
        string deviceToken,
        string errorMessage, 
        CancellationToken cancellationToken);

    Task<ConsumerResult> SetRelayStateAsync(
        ChangeRelayStateEvent command, 
        CancellationToken cancellationToken);

    Task<Result<bool>> ToggleRelayModeAsync(
        Guid relayId, 
        CancellationToken cancellationToken);

    Task<Result<bool>> ToggleRelayStateAsync(
        Guid relayId, 
        CancellationToken cancellationToken);

    Task<Result> DeleteCompletedCommandsAsync(
        CancellationToken cancellationToken);
}