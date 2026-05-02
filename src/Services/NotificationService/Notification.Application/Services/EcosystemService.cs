using Contracts.Events.EcosystemEvents;
using Contracts.Results;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;

namespace Notification.Application.Services;

public sealed class EcosystemService(
    IEcosystemRepository ecosystemRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : IEcosystemService
{
    public async Task<ConsumerResult> CreateAquariumFromEventAsync(
        EcosystemCreatedEvent ecosystemCreated,
        CancellationToken cancellationToken)
    {
        var existingEcosystem = await ecosystemRepository
            .GetByIdAsync(ecosystemCreated.EcosystemId, cancellationToken);

        if (existingEcosystem is not null)
        {
            return ConsumerResult.Success();
        }

        var existingUser = await userRepository
            .ExistsAsync(ecosystemCreated.UserId, cancellationToken);

        if (!existingUser)
        {
            return ConsumerResult
                .RetryableError($"User {ecosystemCreated.UserId} not found. ");
        }

        var (ecosystem, errors) = EcosystemEntity.Create(
            ecosystemCreated.EcosystemId,
            ecosystemCreated.UserId,
            ecosystemCreated.Name,
            ecosystemCreated.OccurredOn);

        if (ecosystem is null)
        {
            return ConsumerResult
                .FatalError($"Failed to create {nameof(EcosystemEntity)}: {string.Join(", ", errors)}");
        }

        await ecosystemRepository.AddAsync(ecosystem, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }

    public async Task<ConsumerResult> UpdateAquariumFromEventAsync(
        EcosystemUdatedEvent ecosystemUpdated,
        CancellationToken cancellationToken)
    {
        var existingEcosystem = await ecosystemRepository
            .GetByIdAsync(ecosystemUpdated.EcosystemId, cancellationToken);

        if (existingEcosystem is null)
        {
            var existingUser = await userRepository
            .ExistsAsync(ecosystemUpdated.UserId, cancellationToken);

            if (!existingUser)
            {
                return ConsumerResult
                    .RetryableError($"User {ecosystemUpdated.UserId} not found. ");
            }

            var (ecosystem, errors) = EcosystemEntity.Create(
                ecosystemUpdated.EcosystemId,
                ecosystemUpdated.UserId,
                ecosystemUpdated.Name,
                ecosystemUpdated.CreatedAt);

            if (ecosystem is null)
            {
                return ConsumerResult
                    .FatalError($"Failed to create {nameof(EcosystemEntity)}: {string.Join(", ", errors)}");
            }

            await ecosystemRepository.AddAsync(ecosystem, cancellationToken);

            return ConsumerResult.Success();
        }

        existingEcosystem.SetName(ecosystemUpdated.Name);

        await ecosystemRepository.UpdateAsync(existingEcosystem, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }

    public async Task<ConsumerResult> DeleteAquariumFromEventAsync(
        EcosystemDeletedEvent ecosystemDeleted,
        CancellationToken cancellationToken)
    {
        await ecosystemRepository.DeleteAsync(ecosystemDeleted.EcosystemId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }
}
