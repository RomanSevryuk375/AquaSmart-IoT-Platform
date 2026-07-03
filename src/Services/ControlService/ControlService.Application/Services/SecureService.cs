using Contracts.Results;
using Control.Application.Interfaces;
using Control.Domain.Entities;
using Control.Domain.Interfaces;

namespace Control.Application.Services;

public sealed class SecureService(
    IEcosystemRepository ecosystemRepository,
    IRelayRepository relayRepository,
    IUserContext userContext) : ISecureService
{
    public async Task<Result<Ecosystem>> EnsureUserOwnsEcosystemAsync(
        Guid ecosystemId,
        CancellationToken cancellationToken)
    {
        Ecosystem? ecosystem = await ecosystemRepository.GetByIdAsync(
            ecosystemId, cancellationToken);
        if (ecosystem is null)
        {
            return Result<Ecosystem>.Failure(Error.NotFound(
                    "Ecosystem.NotFound",
                    $"Ecosystem {ecosystemId} not found. "));
        }

        if (ecosystem.UserId != userContext.UserId)
        {
            return Result<Ecosystem>.Failure(Error.Conflict(
                    "Access.Denied",
                    "You are not the owner of this ecosystem"));
        }

        return Result<Ecosystem>.Success(ecosystem);
    }

    public async Task<Result> EnsureEcosystemOwnsRelayAsync(
        Guid ecosystemId,
        Guid relayId,
        CancellationToken cancellationToken)
    {
        Relay? existingRelay = await relayRepository.GetByIdAsync(
            relayId, cancellationToken);
        if (existingRelay is null)
        {
            return Result<Guid>
                .Failure(Error.NotFound(
                    "Relay.NotFound",
                    $"Relay {relayId} not found"));
        }

        if (existingRelay.EcosystemId != ecosystemId)
        {
            return Result<Guid>
                .Failure(Error.Validation(
                    "Rule.InvalidRelay",
                    "Target relay must belong to the same ecosystem as the rule."));
        }

        return Result.Success();
    }
}
