using Contracts.Results;
using Control.Domain.Entities;

namespace Control.Application.Interfaces;

public interface ISecureService
{
    public Task<Result<Ecosystem>> EnsureUserOwnsEcosystemAsync(
        Guid ecosystemId,
        CancellationToken cancellationToken);

    public Task<Result> EnsureEcosystemOwnsRelayAsync(
        Guid ecosystemId,
        Guid relayId,
        CancellationToken cancellationToken);
}