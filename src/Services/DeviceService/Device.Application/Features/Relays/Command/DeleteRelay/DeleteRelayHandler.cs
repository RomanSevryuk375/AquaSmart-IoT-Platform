namespace Device.Application.Features.Relays.Command.DeleteRelay;

internal sealed class DeleteRelayHandler(
    IRelayRepository relayRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteRelayCommand, Result>
{
    public async Task<Result> Handle(
        DeleteRelayCommand request,
        CancellationToken cancellationToken)
    {
        Relay? existingRelay = await relayRepository.GetByIdAsync(
            request.RelayId, cancellationToken);

        existingRelay!.MarkAsDeleted();

        await relayRepository.DeleteAsync(request.RelayId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
