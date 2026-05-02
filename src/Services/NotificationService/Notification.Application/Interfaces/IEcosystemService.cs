using Contracts.Events.EcosystemEvents;
using Contracts.Results;

namespace Notification.Application.Interfaces;

public interface IEcosystemService
{
    Task<ConsumerResult> CreateAquariumFromEventAsync(
        EcosystemCreatedEvent ecosystemCreated, 
        CancellationToken cancellationToken);

    Task<ConsumerResult> DeleteAquariumFromEventAsync(
        EcosystemDeletedEvent ecosystemDeleted,
        CancellationToken cancellationToken);

    Task<ConsumerResult> UpdateAquariumFromEventAsync(
        EcosystemUdatedEvent ecosystemUpdated, 
        CancellationToken cancellationToken);
}