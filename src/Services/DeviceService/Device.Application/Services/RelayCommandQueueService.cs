using AutoMapper;
using Contracts.Enums;
using Contracts.Events.RelayEvents;
using Contracts.Exceptions;
using Contracts.Results;
using Device.Application.DTOs.RelayCommands;
using Device.Application.Interfaces;
using Device.Domain.Entities;
using Device.Domain.Factories;
using Device.Domain.Interfaces;
using MassTransit;

namespace Device.Application.Services;

public class RelayCommandQueueService(
    IRelayRepository relayRepository,
    IControllerRepository controllerRepository,
    IRelayCommandsQueueRepository queueRepository,
    IMapper mapper,
    IUnitOfWork unitOfWork,
    IMyHasher myHasher,
    IPublishEndpoint publisherEndpoint) : IRelayCommandQueueService
{
    public async Task<IReadOnlyList<RelayCommandResponseDto>> GetPendingCommands(
        Guid controllerId,
        string deviceToken,
        CancellationToken cancellationToken)
    {
        var commands = await queueRepository
            .GetPendingByControllerIdAsync(controllerId, cancellationToken);

        var controller = await controllerRepository
            .GetByIdAsync(controllerId, cancellationToken)
            ?? throw new NotFoundException($"Controller {controllerId} not found");

        if(!myHasher.Verify(deviceToken, controller.DeviceTokenHash))
        {
            throw new InvalidCredentialsException("DeviceToken is not verified.");
        }

        foreach (var command in commands)
        {
            command.MarkAsSent();
            await queueRepository.UpdateAsync(command, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<IReadOnlyList<RelayCommandResponseDto>>(commands);
    }

    public async Task MarkAsCompletedByIdAsync(
        Guid commandId,
        string deviceToken,
        CancellationToken cancellationToken)
    {
        var command = await queueRepository
            .GetByIdAsync(commandId, cancellationToken)
            ?? throw new NotFoundException($"Command {commandId} not found");

        var existingRelay = await relayRepository
            .GetByIdAsync(command.RelayId, cancellationToken)
            ?? throw new NotFoundException($"Relay {command.RelayId} not found");

        var controller = await controllerRepository
            .GetByIdAsync(command.ControllerId, cancellationToken)
            ?? throw new NotFoundException($"Controller {command.ControllerId} not found");

        if (!myHasher.Verify(deviceToken, controller.DeviceTokenHash))
        {
            throw new InvalidCredentialsException("DeviceToken is not verified.");
        }

        existingRelay.SetState(StateEvaluatorFactory.EvaluateEnum(command.Action));
        await relayRepository.UpdateAsync(existingRelay, cancellationToken);

        if (command.Status == CommandStatusEnum.Completed)
        {
            return;
        }

        command.MarkAsCompleted();

        await queueRepository.UpdateAsync(command, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAsFailedByIdAsync(
        Guid commandId,
        string deviceToken,
        string errorMessage,
        CancellationToken cancellationToken)
    {
        var command = await queueRepository
            .GetByIdAsync(commandId, cancellationToken)
            ?? throw new NotFoundException($"Command {commandId} not found");

        var controller = await controllerRepository
            .GetByIdAsync(command.ControllerId, cancellationToken)
            ?? throw new NotFoundException($"Controller {command.ControllerId} not found");

        if (!myHasher.Verify(deviceToken, controller.DeviceTokenHash))
        {
            throw new InvalidCredentialsException("DeviceToken is not verified.");
        }

        if (command.Status == CommandStatusEnum.Failed)
        {
            return;
        }

        command.MarkAsFailed(errorMessage);

        await queueRepository.UpdateAsync(command, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<ConsumerResult> SetRelayStateAsync(
        ChangeRelayStateCommand command,
        CancellationToken cancellationToken)
    {
        var existingRelay = await relayRepository
            .GetByIdAsync(command.RelayId, cancellationToken);

        if (existingRelay is null)
        {
            return ConsumerResult.FatalError($"Relay {command.RelayId} not found");
        }

        var existingController = await controllerRepository
            .GetByIdAsync(command.ControllerId, cancellationToken);

        if (existingController is null)
        {
            return ConsumerResult.FatalError($"Controller {command.ControllerId} not found");
        }

        if (existingRelay.IsManual ||
           (command.ExpireAt.HasValue && command.ExpireAt.Value < DateTime.UtcNow) ||
           existingRelay.IsActive == StateEvaluatorFactory.EvaluateEnum(command.Action))
        {
            return ConsumerResult.FatalError($"Command is unavalible or was expired.");
        }

        var (newCommand, errors) = RelayCommandsQueueEntity.Create(
            existingController.Id,
            existingRelay.Id,
            command.Action,
            command.ExpireAt);

        if (newCommand is null)
        {
            return ConsumerResult
                .FatalError($"Failed to create {nameof(RelayCommandsQueueEntity)}: " +
                $"{string.Join(", ", errors!)}");
        }

        try
        {
            await queueRepository.AddAsync(newCommand, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return ConsumerResult.Success();
        }
        catch (Exception ex)
        {
            return ConsumerResult.RetryableError(ex.Message);
        }
    }

    public async Task<bool> ToggleRelayModeAsync(
        Guid relayId,
        CancellationToken cancellationToken)
    {
        var existingRelay = await relayRepository
            .GetByIdAsync(relayId, cancellationToken)
            ?? throw new NotFoundException($"Relay {relayId} not found");

        existingRelay.ToggleMode();

        await relayRepository.UpdateAsync(existingRelay, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await publisherEndpoint.Publish(new ChangeRelayModeCommand
        {
            RelayId = existingRelay.Id,
            IsManual = existingRelay.IsManual,
        }, cancellationToken);

        return existingRelay.IsManual;
    }

    public async Task<bool> ToggleRelayStateAsync(
        Guid relayId,
        CancellationToken cancellationToken)
    {
        var existingRelay = await relayRepository
            .GetByIdAsync(relayId, cancellationToken)
            ?? throw new NotFoundException($"Relay {relayId} not found");

        existingRelay.ToggleState();

        await relayRepository.UpdateAsync(existingRelay, cancellationToken);

        var (newCommand, errors) = RelayCommandsQueueEntity.Create(
            existingRelay.ControllerId,
            existingRelay.Id,
            StateEvaluatorFactory.EvaluateBool(existingRelay.IsActive),
            DateTime.UtcNow.AddMinutes(15));

        if (newCommand is null)
        {
            throw new DomainValidationException(
                $"Failed to create {nameof(RelayCommandsQueueEntity)}: {string.Join(", ", errors!)}");
        }

        await queueRepository.AddAsync(newCommand, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return existingRelay.IsActive;
    }

    public async Task DeleteCompletedCommandsAsync(
        CancellationToken cancellationToken)
    {
        await queueRepository.DeleteCompletedAsync(cancellationToken);
    }
}
