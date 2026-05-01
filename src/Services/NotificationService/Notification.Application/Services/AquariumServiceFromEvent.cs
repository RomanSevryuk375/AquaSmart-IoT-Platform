using Contracts.Events.AquariumEvents;
using Contracts.Exceptions;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;

namespace Notification.Application.Services;

public class AquariumServiceFromEvent(
    IAquariumRepository aquariumRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : IAquariumServiceFromEvent
{
    public async Task CreateAquariumFromEventAsync(
        AquariumCreatedEvend aquariumCreated,
        CancellationToken cancellationToken)
    {
        var existingAquarium = await aquariumRepository
            .GetByIdAsync(aquariumCreated.AquriumId, cancellationToken);

        if (existingAquarium is not null)
        {
            return;
        }

        var existingUser = await userRepository
            .ExistsAsync(aquariumCreated.UserId, cancellationToken);

        if (!existingUser)
        {
            return;
        }

        var (aquarium, errors) = EcosystemEntity.Create(
            aquariumCreated.AquriumId,
            aquariumCreated.UserId,
            aquariumCreated.Name,
            aquariumCreated.CreatedAt);

        if (aquarium is null)
        {
            throw new DomainValidationException(
                $"Failed to create {nameof(EcosystemEntity)}: {string.Join(", ", errors)}");
        }

        await aquariumRepository.AddAsync(aquarium, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAquariumFromEventAsync(
        AquarimUdatedEvent aquarimUdated,
        CancellationToken cancellationToken)
    {
        var existingAquarium = await aquariumRepository
            .GetByIdAsync(aquarimUdated.AquriumId, cancellationToken);

        if (existingAquarium is null)
        {
            var existingUser = await userRepository
            .ExistsAsync(aquarimUdated.UserId, cancellationToken);

            if (!existingUser)
            {
                return;
            }

            var (aquarium, errors) = EcosystemEntity.Create(
                aquarimUdated.AquriumId,
                aquarimUdated.UserId,
                aquarimUdated.Name,
                aquarimUdated.CreatedAt);

            if (aquarium is null)
            {
                throw new DomainValidationException(
                    $"Failed to create {nameof(EcosystemEntity)}: {string.Join(", ", errors)}");
            }

            await aquariumRepository.AddAsync(aquarium, cancellationToken);

            return;
        }

        existingAquarium.SetName(aquarimUdated.Name);

        await aquariumRepository.UpdateAsync(existingAquarium, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAquariumFromEventAsync(
        AquariumDeletedEvent aquariumDeleted,
        CancellationToken cancellationToken)
    {
        await aquariumRepository.DeleteAsync(aquariumDeleted.AquriumId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
