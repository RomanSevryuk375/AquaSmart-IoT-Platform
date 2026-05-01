using Contracts.Events.RelayEvents;
using Contracts.Results;

namespace Control.Application.Interfaces;

public interface IRelayService
{
    Task<ConsumerResult> ChangedModeAsync(
        ChangeRelayModeCommand relay, 
        CancellationToken cancellationToken);

    Task<ConsumerResult> ChangedStateAsync(
        ChangeRelayStateCommand relay, 
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