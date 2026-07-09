using Contracts.Events.EcosystemEvents;
using Contracts.Results;

namespace Telemetry.Application.Interfaces;

public interface IEcosystemService
{
    public Task<ConsumerResult> CreateEcosystemAsync(
        EcosystemCreatedEvent ecosystem,
        CancellationToken cancellationToken);
    public Task<ConsumerResult> DeleteEcosystemAsync(
        EcosystemDeletedEvent ecosystem,
        CancellationToken cancellationToken);
}