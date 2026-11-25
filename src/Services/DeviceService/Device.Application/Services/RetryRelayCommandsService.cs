using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Constants;
using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Domain.Results;
using Device.Application.Extesions;
using Device.Application.Interfaces;
using Device.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Device.Application.Services;

public sealed class RetryRelayCommandsService(
    IRelayCommandsRepository commandsRepository,
    IControllerRepository controllerRepository,
    IMqttCommandPublisher mqttCommandPublisher,
    IUnitOfWork unitOfWork,
    IOptions<DeviceSettings>? deviceOptions,
    ILogger<RetryRelayCommandsService> logger) : IRelayCommandRetryService
{
    private readonly DeviceSettings _settings = deviceOptions?.Value ?? new DeviceSettings();

    public async Task<Result<int>> ProcessActiveCommandsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            IReadOnlyList<RelayCommand> activeCommands = await commandsRepository.GetActiveCommandsAsync(cancellationToken);
            if (activeCommands.Count == 0)
            {
                return Result<int>.Success(0);
            }

            DateTime now = DateTime.UtcNow;
            TimeSpan retryCooldown = TimeSpan.FromSeconds(Math.Max(1, _settings.CommandRetryCooldownSeconds));
            var controllerCache = new Dictionary<Guid, Controller?>();
            int processedCount = 0;

            foreach (RelayCommand command in activeCommands)
            {
                if (command.ExpireAt.HasValue && command.ExpireAt.Value <= now)
                {
                    Result expireResult = command.Expire();
                    if (expireResult.IsSuccess)
                    {
                        processedCount++;
                    }

                    continue;
                }

                if (command.Status == CommandStatus.Sent &&
                    command.AttemptCount >= RelayCommandConstants.MaxAttemptCount)
                {
                    if (!command.ProcessedAt.HasValue || (now - command.ProcessedAt.Value) >= retryCooldown)
                    {
                        Result expireResult = command.Expire();
                        if (expireResult.IsSuccess)
                        {
                            processedCount++;
                        }
                    }

                    continue;
                }

                if (command.Status == CommandStatus.Pending &&
                    (now - command.CreatedAt) < retryCooldown)
                {
                    continue;
                }

                if (command.Status == CommandStatus.Sent &&
                    command.ProcessedAt.HasValue &&
                    (now - command.ProcessedAt.Value) < retryCooldown)
                {
                    continue;
                }

                Controller? controller = await ResolveControllerAsync(command.ControllerId, controllerCache, cancellationToken);
                if (controller is null || !controller.IsOnline)
                {
                    continue;
                }

                var payload = new RelayCommandMqttPayload
                {
                    CommandId = command.Id,
                    RelayId = command.RelayId,
                    TargetState = command.TargetState,
                    ExpireAt = command.ExpireAt,
                    CreatedAt = command.CreatedAt,
                };

                bool published = await mqttCommandPublisher.PublishRelayCommandAsync(
                    controller.MacAddress.Value, payload, cancellationToken);

                if (published)
                {
                    command.MarkAsSent();
                    processedCount++;
                }
                else
                {
                    logger.LogWarning(
                        "Retry publish skipped for RelayCommand {CommandId}: MQTT broker not connected.",
                        command.Id);
                }
            }

            if (processedCount > 0)
            {
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return Result<int>.Success(processedCount);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure(Error.Failure(ErrorMessages.DatabaseError, ex.Message));
        }
    }

    public async Task<Result<int>> RedeliverPendingForControllerAsync(
        string macAddress,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Result<MacAddress> macResult = MacAddress.Create(macAddress);
            if (macResult.IsFailure)
            {
                return Result<int>.Success(0);
            }

            Controller? controller = await controllerRepository.GetByMacAddressAsync(
                macResult.Value.Value, cancellationToken);
            if (controller is null || !controller.IsOnline)
            {
                return Result<int>.Success(0);
            }

            DateTime now = DateTime.UtcNow;
            IReadOnlyList<RelayCommand> pendingCommands = await commandsRepository.GetUnexpiredPendingByControllerIdAsync(
                controller.Id, now, cancellationToken);
            if (pendingCommands.Count == 0)
            {
                return Result<int>.Success(0);
            }

            int deliveredCount = 0;
            foreach (RelayCommand command in pendingCommands)
            {
                var payload = new RelayCommandMqttPayload
                {
                    CommandId = command.Id,
                    RelayId = command.RelayId,
                    TargetState = command.TargetState,
                    ExpireAt = command.ExpireAt,
                    CreatedAt = command.CreatedAt,
                };

                bool published = await mqttCommandPublisher.PublishRelayCommandAsync(
                    controller.MacAddress.Value, payload, cancellationToken);

                if (published)
                {
                    command.MarkAsSent();
                    deliveredCount++;
                }
            }

            if (deliveredCount > 0)
            {
                await unitOfWork.SaveChangesAsync(cancellationToken);
                logger.LogInformation(
                    "Redelivered {DeliveredCount} pending relay command(s) to reconnected Controller {ControllerId}.",
                    deliveredCount,
                    controller.Id);
            }

            return Result<int>.Success(deliveredCount);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure(Error.Failure(ErrorMessages.DatabaseError, ex.Message));
        }
    }

    private async Task<Controller?> ResolveControllerAsync(
        Guid controllerId,
        Dictionary<Guid, Controller?> cache,
        CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(controllerId, out Controller? cached))
        {
            return cached;
        }

        Controller? loaded = await controllerRepository.GetByIdAsync(controllerId, cancellationToken);
        cache[controllerId] = loaded;
        return loaded;
    }
}
