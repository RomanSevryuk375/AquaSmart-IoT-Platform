using Contracts.Events.RelayEvents;
using Contracts.Results;
using Device.Application.DTOs.RelayCommands;

namespace Device.Application.Interfaces;

public interface IRelayCommandQueueService
{
    Task<IReadOnlyList<RelayCommandResponseDto>> GetPendingCommands(
        Guid controllerId,
        string deviceToken,
        CancellationToken cancellationToken);

    Task MarkAsCompletedByIdAsync(
        Guid commandId,
        string deviceToken,
        CancellationToken cancellationToken);

    Task MarkAsFailedByIdAsync(
        Guid commandId,
        string deviceToken,
        string errorMessage, 
        CancellationToken cancellationToken);

    Task<ConsumerResult> SetRelayStateAsync(
        ChangeRelayStateCommand command, 
        CancellationToken cancellationToken);

    Task<bool> ToggleRelayModeAsync(
        Guid relayId, 
        CancellationToken cancellationToken);

    Task<bool> ToggleRelayStateAsync(
        Guid relayId, 
        CancellationToken cancellationToken);

    Task DeleteCompletedCommandsAsync(
        CancellationToken cancellationToken);
}