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
        Ecosystem? existingEcosystem = await ecosystemRepository.GetByIdAsync(
            ecosystem.EcosystemId, cancellationToken);
        if (existingEcosystem is not null)
        {
            return ConsumerResult.Success();
        }

        Result<Ecosystem> result = Ecosystem.Create(
            ecosystem.EcosystemId,
            ecosystem.ControllerId,
            ecosystem.UserId);
        if (result.IsFailure)
        {
            return ConsumerResult.FatalError($"Ecosystem creation failed: {result.Error.Message}");
        }

        await ecosystemRepository.AddAsync(result.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }

    public async Task<ConsumerResult> DeleteEcosystemAsync(
        EcosystemDeletedEvent ecosystem,
        CancellationToken cancellationToken)
    {
        Ecosystem? existingEcosystem = await ecosystemRepository.GetByIdAsync(
            ecosystem.EcosystemId, cancellationToken);
        if (existingEcosystem is null)
        {
            return ConsumerResult.Success();
        }

        await ecosystemRepository.DeleteAsync(existingEcosystem.Id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }
}
