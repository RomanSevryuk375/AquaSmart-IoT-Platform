using BuildingBlocks.Domain.Results;

namespace Device.Application.Features.Relays.Command.DeleteRelay;

public sealed class DeleteRelayHandler(
    IRelayRepository relayRepository,
    IControllerRepository controllerRepository)
    : IRequestHandler<DeleteRelayCommand, Result>
{
    public async Task<Result> Handle(
        DeleteRelayCommand request,
        CancellationToken cancellationToken)
    {
        Relay? existingRelay = await relayRepository.GetByIdAsync(
            request.RelayId, cancellationToken);

        Guid controllerId = existingRelay!.ControllerId;
        existingRelay.MarkAsDeleted(request.UserId);

        await relayRepository.DeleteAsync(request.RelayId, cancellationToken);

        Controller? controller = await controllerRepository.GetByIdAsync(controllerId, cancellationToken);
        controller?.IncrementConfigVersion("relay_deleted");

        return Result.Success();
    }
}
