namespace Device.Application.Features.RelayCommands.Command.DeleteCompleted;

internal class DeleteCompleted(
    IRelayCommandsRepository commandsRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteCompletedCommand, Result>
{
    public async Task<Result> Handle(
        DeleteCompletedCommand request,
        CancellationToken cancellationToken)
    {
        await commandsRepository.DeleteCompletedAsync(cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
