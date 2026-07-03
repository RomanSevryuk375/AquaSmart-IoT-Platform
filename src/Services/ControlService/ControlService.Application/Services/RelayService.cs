using Contracts.Enums;
using Contracts.Events.RelayEvents;
using Contracts.Results;
using Control.Application.Interfaces;
using Control.Domain.Entities;
using Control.Domain.Interfaces;

namespace Control.Application.Services;

public sealed class RelayService(
    IRelayRepository relayRepository,
    IEcosystemRepository ecosystemRepository,
    IUnitOfWork unitOfWork) : IRelayService
{
    public async Task<ConsumerResult> ChangedModeAsync(
        RelayModeChangedEvent relay,
        CancellationToken cancellationToken)
    {
        Relay? existingRelay = await relayRepository.GetByIdAsync(
            relay.RelayId, cancellationToken);
        if (existingRelay is null)
        {
            return ConsumerResult
                .RetryableError($"Relay {relay.RelayId} not found. ");
        }

        existingRelay.SetMode(relay.IsManual);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }

    public async Task<ConsumerResult> ChangedStateAsync(
        ChangeRelayStateEvent relay,
        CancellationToken cancellationToken)
    {
        DateTime expireAt = DateTime.UtcNow.AddMinutes(5);

        Relay? existingRelay = await relayRepository.GetByIdAsync(
            relay.RelayId, cancellationToken);
        if (existingRelay is null)
        {
            return ConsumerResult
                .RetryableError($"Relay {relay.RelayId} not found. ");
        }

        existingRelay.SetState(relay.TargetState, expireAt);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }

    public async Task<ConsumerResult> CreateRelayAsync(
        RelayCreatedEvent newRelay,
        CancellationToken cancellationToken)
    {
        if (await relayRepository.ExistsAsync(newRelay.RelayId, cancellationToken))
        {
            return ConsumerResult.Success();
        }

        var relayForm = new RelayForm(newRelay.RelayId, newRelay.ControllerId,
            newRelay.PowerSensorId, newRelay.Name, newRelay.Purpose, newRelay.IsManual,
            newRelay.IsActive, newRelay.CreatedAt);

        ConsumerResult creationResult = await CreateValidRealyAsync(relayForm, cancellationToken);
        if (!creationResult.IsSuccess)
        {
            return creationResult;
        }

        return ConsumerResult.Success();
    }

    public async Task<ConsumerResult> DeletedRelayAsync(
        RelayDeletedEvent relayDeleted,
        CancellationToken cancellationToken)
    {
        Relay? existingRelay = await relayRepository
            .GetByIdAsync(relayDeleted.RelayId, cancellationToken);

        if (existingRelay is null)
        {
            return ConsumerResult.Success();
        }

        await relayRepository.DeleteAsync(relayDeleted.RelayId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }

    public async Task<ConsumerResult> UpdatedRelayAsync(
        RelayUpdatedEvent relayUpdated,
        CancellationToken cancellationToken)
    {
        Relay? existingRelay = await relayRepository
            .GetByIdAsync(relayUpdated.RelayId, cancellationToken);

        if (existingRelay is null)
        {
            var relayForm = new RelayForm(
            relayUpdated.RelayId,
            relayUpdated.ControllerId,
            relayUpdated.PowerSensorId,
            relayUpdated.Name,
            relayUpdated.Purpose,
            relayUpdated.IsManual,
            relayUpdated.IsActive,
            relayUpdated.CreatedAt);

            ConsumerResult creationResult = await CreateValidRealyAsync(relayForm, cancellationToken);
            if (!creationResult.IsSuccess)
            {
                return creationResult;
            }

            return ConsumerResult.Success();
        }

        DateTime expireAt = DateTime.UtcNow.AddMinutes(5);

        existingRelay!.SetPurpose(relayUpdated.Purpose);
        existingRelay.SetMode(relayUpdated.IsManual);
        existingRelay.SetState(relayUpdated.IsActive, expireAt);
        existingRelay.SetName(relayUpdated.Name);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }

    private async Task<ConsumerResult> CreateValidRealyAsync(
        RelayForm relayForm,
        CancellationToken cancellationToken)
    {
        Ecosystem? ecosystem = await ecosystemRepository.GetByControllerIdAsync(
            relayForm.ControllerId, cancellationToken);
        if (ecosystem is null)
        {
            return ConsumerResult.RetryableError(
                $"Ecosystem with controller {relayForm.ControllerId} not found. ");
        }

        Result<Relay> result = Relay.Create(
            relayForm.RelayId,
            ecosystem.Id,
            relayForm.ControllerId,
            relayForm.PowerSensorId,
            relayForm.Name,
            relayForm.Purpose,
            relayForm.IsManual,
            relayForm.IsActive,
            relayForm.CreatedAt);

        if (result.IsFailure)
        {
            return ConsumerResult.FatalError($"{result.Error}");
        }

        await relayRepository.AddAsync(result.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ConsumerResult.Success();
    }

    private record RelayForm(Guid RelayId,
                Guid ControllerId,
                Guid? PowerSensorId,
                string Name,
                RelayPurpose Purpose,
                bool IsManual,
                bool IsActive,
                DateTime CreatedAt);
}
