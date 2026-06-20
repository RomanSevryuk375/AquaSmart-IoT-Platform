using Device.Application.Interfaces;

namespace Device.Application.Features.RelayCommands.Command.MarkAsCompleted;

internal sealed class MarkAsCompletedHandler(
    IRelayCommandsRepository queueRepository,
    IRelayRepository relayRepository,
    IUnitOfWork unitOfWork,
    IDeviceSecurityService securityService) : IRequestHandler<MarkAsCompletedCommand, Result>
{
    public async Task<Result> Handle(
        MarkAsCompletedCommand request,
        CancellationToken cancellationToken)
    {
        RelayCommand? command = await queueRepository.GetByIdAsync(
            request.CommandId, cancellationToken);
        if (command is null)
        {
            return Result.Failure(Error.NotFound<RelayCommand>(
                    $"{nameof(RelayCommand)} {request.CommandId} not found"));
        }

        Relay? existingRelay = await relayRepository.GetByIdAsync(
            command.RelayId, cancellationToken);
        if (existingRelay is null)
        {
            return Result.Failure(Error.NotFound(
                    "Relay.NotFound",
                    $"{nameof(Relay)} {command.RelayId} not found"));
        }

        Result ownership = await securityService.EnsureDeviceAccessAsync(
            command.ControllerId, request.DeviceToken, cancellationToken);
        if (ownership.IsFailure)
        {
            return Result.Failure(ownership.Error);
        }

        existingRelay.SetState(command.TargeState);
        if (command.Status == CommandStatus.Completed)
        {
            return Result.Success();
        }

        command.MarkAsCompleted();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
