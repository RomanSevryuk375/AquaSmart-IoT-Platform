namespace Device.Application.Features.RelayCommands.Command.MarkAsCompleted;

internal sealed class MarkAsCompletedHandler(
    IRelayCommandsRepository queueRepository,
    IRelayRepository relayRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<MarkAsCompletedCommand, Result>
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
                    string.Format(ErrorMessages.RelayCommandNotFound, request.CommandId)));
        }

        Relay? existingRelay = await relayRepository.GetByIdAsync(
            command.RelayId, cancellationToken);
        if (existingRelay is null)
        {
            return Result.Failure(Error.NotFound<Relay>(
                    string.Format(ErrorMessages.RelayNotFound, command.RelayId)));
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
