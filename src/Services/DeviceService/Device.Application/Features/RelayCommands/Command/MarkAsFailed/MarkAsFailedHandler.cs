namespace Device.Application.Features.RelayCommands.Command.MarkAsFailed;

internal sealed class MarkAsFailedHandler(
    IRelayCommandsRepository queueRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<MarkAsFailedCommand, Result>
{
    public async Task<Result> Handle(
        MarkAsFailedCommand request,
        CancellationToken cancellationToken)
    {
        RelayCommand? command = await queueRepository.GetByIdAsync(
            request.CommandId, cancellationToken);
        if (command is null)
        {
            return Result.Failure(Error.NotFound<RelayCommand>(
                    $"{nameof(RelayCommand)} {request.CommandId} not found"));
        }

        if (command.Status == CommandStatus.Failed)
        {
            return Result.Success();
        }

        command.MarkAsFailed(request.ErrorMessage);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
