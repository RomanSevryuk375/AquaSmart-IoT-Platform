using AutoMapper;
using Contracts.Enums;
using Contracts.Events.RelayEvents;
using Contracts.Results;
using Device.Application.DTOs.RelayCommands;
using Device.Application.Extesions;
using Device.Application.Interfaces;
using Device.Domain.Entities;
using Device.Domain.Factories;
using Device.Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace Device.Application.Services;

public sealed class RelayCommandQueueService(
    IRelayRepository relayRepository,
    IControllerRepository controllerRepository,
    IRelayCommandsQueueRepository queueRepository,
    IMapper mapper,
    IUnitOfWork unitOfWork,
    IDeviceSecurityService securityService,
    IOptions<DeviceSettings> deviceOptions) : IRelayCommandQueueService
{
    public async Task<Result<IReadOnlyList<RelayCommandResponseDto>>> GetPendingCommands(
        Guid controllerId,
        string deviceToken,
        CancellationToken cancellationToken)
    {     
        var ownership = await securityService.EnsureDeviceAccessAsync(
            controllerId, deviceToken, cancellationToken);

        if (ownership.IsFailure)
        {
            return Result<IReadOnlyList<RelayCommandResponseDto>>
                .Failure(ownership.Error);
        }

        var commands = await queueRepository
            .GetPendingByControllerIdAsync(controllerId, cancellationToken);

        foreach (var command in commands)
        {
            command.MarkAsSent();
            await queueRepository.UpdateAsync(command, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<IReadOnlyList<RelayCommandResponseDto>>.Success(
            mapper.Map<IReadOnlyList<RelayCommandResponseDto>>(commands));
    }

    public async Task<Result> MarkAsCompletedByIdAsync(
        Guid commandId,
        string deviceToken,
        CancellationToken cancellationToken)
    {
        var command = await queueRepository
            .GetByIdAsync(commandId, cancellationToken);

        if (command is null)
        {
            return Result.Failure(Error.NotFound(
                    "Command.NotFound",
                    $"{nameof(RelayCommand)} {commandId} not found"));
        }

        var existingRelay = await relayRepository
            .GetByIdAsync(command.RelayId, cancellationToken);

        if (existingRelay is null)
        {
            return Result.Failure(Error.NotFound(
                    "Relay.NotFound",
                    $"{nameof(Relay)} {command.RelayId} not found"));
        }

        var ownership = await securityService.EnsureDeviceAccessAsync(
            command.ControllerId, deviceToken, cancellationToken);

        if (ownership.IsFailure)
        {
            return Result.Failure(ownership.Error);
        }

        existingRelay.SetState(StateEvaluatorFactory.EvaluateEnum(command.Action));

        await relayRepository.UpdateAsync(existingRelay, cancellationToken);

        if (command.Status == CommandStatusEnum.Completed)
        {
            return Result.Success();
        }

        command.MarkAsCompleted();

        await queueRepository.UpdateAsync(command, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> MarkAsFailedByIdAsync(
        Guid commandId,
        string deviceToken,
        string errorMessage,
        CancellationToken cancellationToken)
    {
        var command = await queueRepository
            .GetByIdAsync(commandId, cancellationToken);

        if (command is null)
        {
            return Result.Failure(Error.NotFound(
                    "Command.NotFound",
                    $"{nameof(RelayCommand)} {commandId} not found"));
        }

        var ownership = await securityService.EnsureDeviceAccessAsync(
            command.ControllerId, deviceToken, cancellationToken);

        if (ownership.IsFailure)
        {
            return Result<IReadOnlyList<RelayCommandResponseDto>>.Failure(ownership.Error);
        }

        if (command.Status == CommandStatusEnum.Failed)
        {
            return Result.Success();
        }

        command.MarkAsFailed(errorMessage);

        await queueRepository.UpdateAsync(command, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<ConsumerResult> SetRelayStateAsync(
        ChangeRelayStateEvent command,
        CancellationToken cancellationToken)
    {
        var existingRelay = await relayRepository
            .GetByIdAsync(command.RelayId, cancellationToken);

        if (existingRelay is null)
        {
            return ConsumerResult
                .FatalError($"Relay {command.RelayId} not found");
        }

        var existingController = await controllerRepository
            .GetByIdAsync(command.ControllerId, cancellationToken);

        if (existingController is null)
        {
            return ConsumerResult
                .FatalError($"Controller {command.ControllerId} not found");
        }

        if (UnavalibleCommand(command, existingRelay))
        {
            return ConsumerResult
                .FatalError($"Command is unavalible or was expired.");
        }

        var newCommand = RelayCommand.Create(
            existingController.Id,
            existingRelay.Id,
            command.Action,
            command.ExpireAt);

        if (newCommand.IsFailure)
        {
            return ConsumerResult.FatalError($"{newCommand.Error}");
        }

        try
        {
            await queueRepository.AddAsync(newCommand.Value, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return ConsumerResult.Success();
        }
        catch (Exception ex)
        {
            return ConsumerResult.RetryableError(ex.Message);
        }
    }

    public async Task<Result<bool>> ToggleRelayModeAsync(
        Guid relayId,
        CancellationToken cancellationToken)
    {
        var existingRelay = await relayRepository
            .GetByIdAsync(relayId, cancellationToken);

        if (existingRelay is null)
        {
            return Result<bool>
                .Failure(Error.NotFound(
                    "Relay.NotFound",
                    $"{nameof(Relay)} {relayId} not found"));
        }

        var ownership = await securityService.EnsureUserOwnsControllerAsync(
            existingRelay.ControllerId, cancellationToken);

        if (ownership.IsFailure)
        {
            return Result<bool>.Failure(ownership.Error);
        }

        existingRelay.SetMode(!existingRelay.IsManual);

        await relayRepository.UpdateAsync(existingRelay, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(existingRelay.IsManual);
    }

    public async Task<Result<bool>> ToggleRelayStateAsync(
        Guid relayId,
        CancellationToken cancellationToken)
    {
        var existingRelay = await relayRepository
            .GetByIdAsync(relayId, cancellationToken);

        if (existingRelay is null)
        {
            return Result<bool>
                .Failure(Error.NotFound(
                    "Relay.NotFound",
                    $"{nameof(Relay)} {relayId} not found"));
        }

        var ownership = await securityService.EnsureUserOwnsControllerAsync(
            existingRelay.ControllerId, cancellationToken);

        if (ownership.IsFailure)
        {
            return Result<bool>.Failure(ownership.Error);
        }

        existingRelay.SetState(!existingRelay.IsActive);

        await relayRepository.UpdateAsync(existingRelay, cancellationToken);

        var newCommand = RelayCommand.Create(
            existingRelay.ControllerId,
            existingRelay.Id,
            StateEvaluatorFactory.EvaluateBool(existingRelay.IsActive),
            DateTime.UtcNow.AddMinutes(deviceOptions.Value.CommandTtlMinutes));

        if (newCommand.IsFailure)
        {
            return Result<bool>.Failure(newCommand.Error);
        }

        await queueRepository.AddAsync(newCommand.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(existingRelay.IsActive);
    }

    public async Task<Result> DeleteCompletedCommandsAsync(
        CancellationToken cancellationToken)
    {
        await queueRepository.DeleteCompletedAsync(cancellationToken);

        return Result.Success();
    }

    private static bool UnavalibleCommand(
        ChangeRelayStateEvent command, 
        Relay existingRelay)
    {
        return existingRelay.IsManual ||
              (command.ExpireAt.HasValue && command.ExpireAt.Value < DateTime.UtcNow) ||
               existingRelay.IsActive == StateEvaluatorFactory.EvaluateEnum(command.Action);
    }
}
