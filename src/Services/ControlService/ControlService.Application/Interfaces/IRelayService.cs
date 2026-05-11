using Contracts.Events.RelayEvents;
using Contracts.Results;

namespace Control.Application.Interfaces;

public interface IRelayService
{
    Task<ConsumerResult> ChangedModeAsync(
        RelayModeChangedEvent relay, 
        CancellationToken cancellationToken);

    Task<ConsumerResult> ChangedStateAsync(
        ChangeRelayStateEvent relay, 
        CancellationToken cancellationToken);

    Task<ConsumerResult> CreateRelayAsync(
        RelayCreatedEvent newRelay, 
        CancellationToken cancellationToken);

    Task<ConsumerResult> DeletedRelayAsync(
        RelayDeletedEvent relayDeleted, 
        CancellationToken cancellationToken);

    Task<ConsumerResult> UpdatedRelayAsync(
        RelayUpdatedEvent relayUpdated, 
        CancellationToken cancellationToken);
}