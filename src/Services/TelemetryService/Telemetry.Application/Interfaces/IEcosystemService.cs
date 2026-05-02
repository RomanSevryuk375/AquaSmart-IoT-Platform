using Contracts.Events.EcosystemEvents;
using Contracts.Results;

namespace Telemetry.Application.Interfaces;

public interface IEcosystemService
{
    Task<ConsumerResult> CreateEcosystemAsync(
        EcosystemCreatedEvent ecosystem, 
        CancellationToken cancellationToken);
    Task<ConsumerResult> DeleteEcosystemAsync(
        EcosystemDeletedEvent ecosystem, 
        CancellationToken cancellationToken);
}