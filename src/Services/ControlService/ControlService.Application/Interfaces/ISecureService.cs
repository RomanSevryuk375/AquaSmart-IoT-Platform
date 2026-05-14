using Contracts.Results;
using Control.Domain.Entities;

namespace Control.Application.Interfaces;

public interface ISecureService
{
    Task<Result<EcosystemEntity>> EnsureUserOwnsEcosystemAsync(
        Guid ecosystemId, 
        CancellationToken cancellationToken);
}