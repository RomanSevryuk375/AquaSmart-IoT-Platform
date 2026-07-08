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
        Ecosystem? existingEcosystem = await ecosystemRepository
            .GetByIdAsync(ecosystemCreated.EcosystemId, cancellationToken);

        if (existingEcosystem is not null)
        {
            return ConsumerResult.Success();
        }

        bool existingUser = await userRepository
            .ExistsAsync(ecosystemCreated.UserId, cancellationToken);

        if (!existingUser)
        {
            return ConsumerResult
                .RetryableError($"User {ecosystemCreated.UserId} not found. ");
        }

        Result<Ecosystem> ecosystemResult = Ecosystem.Create(
            ecosystemCreated.EcosystemId,
            ecosystemCreated.UserId,
            ecosystemCreated.Name);
        if (ecosystemResult.IsFailure)
        {
            return ConsumerResult.FatalError(ecosystemResult.Error.Message);
        }

        await ecosystemRepository.AddAsync(ecosystemResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }

    public async Task<ConsumerResult> UpdateAquariumFromEventAsync(
        EcosystemUdatedEvent ecosystemUpdated,
        CancellationToken cancellationToken)
    {
        Ecosystem? existingEcosystem = await ecosystemRepository
            .GetByIdAsync(ecosystemUpdated.EcosystemId, cancellationToken);

        if (existingEcosystem is null)
        {
            bool existingUser = await userRepository
            .ExistsAsync(ecosystemUpdated.UserId, cancellationToken);

            if (!existingUser)
            {
                return ConsumerResult
                    .RetryableError($"User {ecosystemUpdated.UserId} not found. ");
            }

            Result<Ecosystem> ecosystemResult = Ecosystem.Create(
                ecosystemUpdated.EcosystemId,
                ecosystemUpdated.UserId,
                ecosystemUpdated.Name);
            if (ecosystemResult.IsFailure)
            {
                return ConsumerResult.FatalError(ecosystemResult.Error.Message);
            }

            await ecosystemRepository.AddAsync(ecosystemResult.Value, cancellationToken);

            return ConsumerResult.Success();
        }

        existingEcosystem.SetName(ecosystemUpdated.Name);

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
