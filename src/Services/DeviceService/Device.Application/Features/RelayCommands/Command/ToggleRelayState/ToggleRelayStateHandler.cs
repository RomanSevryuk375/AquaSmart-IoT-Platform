using BuildingBlocks.Domain.Results;
using Device.Application.Extesions;
using MassTransit;
using Microsoft.Extensions.Options;

namespace Device.Application.Features.RelayCommands.Command.ToggleRelayState;

public sealed class ToggleRelayStateHandler(
    IRelayRepository relayRepository,
    IRelayCommandsRepository queueRepository,
    IOptions<DeviceSettings> deviceOptions) : IRequestHandler<ToggleRelayStateCommand, Result<RelayCommandQueuedResponse>>
{
    public async Task<Result<RelayCommandQueuedResponse>> Handle(
        ToggleRelayStateCommand request,
        CancellationToken cancellationToken)
    {
        Relay? existingRelay = await relayRepository.GetByIdAsync(
            request.RelayId, cancellationToken);

        bool targetState = !existingRelay!.IsActive;

        Result<RelayCommand> newCommand = RelayCommand.Create(
            id: NewId.NextGuid(),
            existingRelay.ControllerId,
            existingRelay.Id,
            targetState,
            expireAt: DateTime.UtcNow.AddMinutes(deviceOptions.Value.CommandTtlMinutes));
        if (newCommand.IsFailure)
        {
            return Result<RelayCommandQueuedResponse>.Failure(newCommand.Error);
        }

        await queueRepository.AddAsync(newCommand.Value, cancellationToken);

        return Result<RelayCommandQueuedResponse>.Success(new RelayCommandQueuedResponse
        {
            CommandId = newCommand.Value.Id,
            RelayId = existingRelay.Id,
            TargetState = targetState,
            Status = newCommand.Value.Status.ToString(),
        });
    }
}
