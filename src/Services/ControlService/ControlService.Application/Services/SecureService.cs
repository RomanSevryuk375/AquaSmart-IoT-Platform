using Contracts.Results;
using Control.Application.Interfaces;
using Control.Domain.Entities;
using Control.Domain.Interfaces;

namespace Control.Application.Services;

public sealed class SecureService(
    IEcosystemRepository ecosystemRepository,
    IUserContext userContext) : ISecureService
{
    public async Task<Result<EcosystemEntity>> EnsureUserOwnsEcosystemAsync(
        Guid ecosystemId,
        CancellationToken cancellationToken)
    {
        var ecosystem = await ecosystemRepository
        .GetByIdAsync(ecosystemId, cancellationToken);

        if (ecosystem is null)
        {
            return Result<EcosystemEntity>.Failure(Error.NotFound(
                    "Ecosystem.NotFound",
                    $"Ecosystem {ecosystemId} not found. "));
        }

        if (ecosystem.UserId != userContext.UserId)
        {
            return Result<EcosystemEntity>.Failure(Error.Conflict(
                    "Access.Denied",
                    "You are not the owner of this ecosystem"));
        }

        return Result<EcosystemEntity>.Success(ecosystem);
    }
}
