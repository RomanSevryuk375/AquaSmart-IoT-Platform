
using Device.Application.Extesions;
using Device.Application.Interfaces;
using MassTransit;
using Microsoft.Extensions.Options;

namespace Device.Application.Features.RelayCommands.Command.ToggleRelayState;

internal sealed class ToggleRelayStateHandler(
    IRelayRepository relayRepository,
    IDeviceSecurityService securityService,
    IRelayCommandsRepository queueRepository,
    IUnitOfWork unitOfWork,
    IOptions<DeviceSettings> deviceOptions) : IRequestHandler<ToggleRelayStateCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        ToggleRelayStateCommand request,
        CancellationToken cancellationToken)
    {
        Relay? existingRelay = await relayRepository.GetByIdAsync(
            request.RelayId, cancellationToken);
        if (existingRelay is null)
        {
            return Result<bool>.Failure(Error.NotFound<Relay>(
                    $"{nameof(Relay)} {request.RelayId} not found"));
        }

        Result ownership = await securityService.EnsureUserOwnsControllerAsync(
            existingRelay.ControllerId, cancellationToken);
        if (ownership.IsFailure)
        {
            return Result<bool>.Failure(ownership.Error);
        }

        existingRelay.SetState(!existingRelay.IsActive);

        Result<RelayCommand> newCommand = RelayCommand.Create(
            id: NewId.NextGuid(),
            existingRelay.ControllerId,
            existingRelay.Id,
            existingRelay.IsActive,
            expireAt: DateTime.UtcNow.AddMinutes(deviceOptions.Value.CommandTtlMinutes));
        if (newCommand.IsFailure)
        {
            return Result<bool>.Failure(newCommand.Error);
        }

        await queueRepository.AddAsync(newCommand.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(existingRelay.IsActive);
    }
}
