using Contracts.Events.RelayEvents;
using Contracts.Results;

namespace Control.Application.Interfaces;

public interface IRelayService
{
    public Task<ConsumerResult> ChangedModeAsync(
        RelayModeChangedEvent relay,
        CancellationToken cancellationToken);

    public Task<ConsumerResult> ChangedStateAsync(
        ChangeRelayStateEvent relay,
        CancellationToken cancellationToken);

    public Task<ConsumerResult> CreateRelayAsync(
        RelayCreatedEvent newRelay,
        CancellationToken cancellationToken);

    public Task<ConsumerResult> DeletedRelayAsync(
        RelayDeletedEvent relayDeleted,
        CancellationToken cancellationToken);

    public Task<ConsumerResult> UpdatedRelayAsync(
        RelayUpdatedEvent relayUpdated,
        CancellationToken cancellationToken);
}