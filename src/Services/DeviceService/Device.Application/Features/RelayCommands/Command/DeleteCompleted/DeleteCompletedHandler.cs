namespace Device.Application.Features.RelayCommands.Command.DeleteCompleted;

internal class DeleteCompletedHandler(
    IRelayCommandsRepository commandsRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteCompletedCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        DeleteCompletedCommand request,
        CancellationToken cancellationToken)
    {
        int deletedCount = await commandsRepository.DeleteCompletedAsync(cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(deletedCount);
    }
}
