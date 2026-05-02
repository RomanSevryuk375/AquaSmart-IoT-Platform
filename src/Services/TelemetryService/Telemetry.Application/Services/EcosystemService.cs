using Contracts.Events.EcosystemEvents;
using Contracts.Results;
using Telemetry.Application.Interfaces;
using Telemetry.Domain.Entities;
using Telemetry.Domain.Interfaces;

namespace Telemetry.Application.Services;

public sealed class EcosystemService(
    IEcosystemRepository ecosystemRepository,
    IUnitOfWork unitOfWork) : IEcosystemService
{
    public async Task<ConsumerResult> CreateEcosystemAsync(
        EcosystemCreatedEvent ecosystem,
        CancellationToken cancellationToken)
    {
        var existingEcosystem = await ecosystemRepository
            .GetByIdAsync(ecosystem.EcosystemId, cancellationToken);

        if (existingEcosystem is not null)
        {
            return ConsumerResult.Success();
        }

        var (newEcosystem, errors) = EcosystemEntity.Create(
            ecosystem.EcosystemId,
            ecosystem.ControllerId,
            ecosystem.UserId);

        if (newEcosystem is null)
        {
            return ConsumerResult
                .FatalError($"Failed to create {nameof(EcosystemEntity)}: " +
                $"{string.Join(", ", errors!)}");
        }

        await ecosystemRepository.AddAsync(newEcosystem, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }

    public async Task<ConsumerResult> DeleteEcosystemAsync(
        EcosystemDeletedEvent ecosystem,
        CancellationToken cancellationToken)
    {
        var existingEcosystem = await ecosystemRepository
            .GetByIdAsync(ecosystem.EcosystemId, cancellationToken);

        if (existingEcosystem is null)
        {
            return ConsumerResult
                .RetryableError($"Ecosystem {ecosystem.EcosystemId} not found.");
        }

        await ecosystemRepository.DeleteAsync(existingEcosystem.Id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }
}
