
using Device.Application.Interfaces;

namespace Device.Application.Features.Relays.Command.DeleteRelay;

internal sealed class DeleteRelayHandler(
    IRelayRepository relayRepository,
    IDeviceSecurityService securityService,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteRelayCommand, Result>
{
    public async Task<Result> Handle(
        DeleteRelayCommand request,
        CancellationToken cancellationToken)
    {
        Relay? existingRelay = await relayRepository.GetByIdAsync(
            request.RelayId, cancellationToken);
        if (existingRelay is null)
        {
            return Result.Failure(Error.NotFound<Relay>(
                    $"{nameof(Relay)} {request.RelayId} not found"));
        }

        Result ownership = await securityService.EnsureUserOwnsControllerAsync(
            existingRelay.ControllerId, cancellationToken);

        if (ownership.IsFailure)
        {
            return Result.Failure(ownership.Error);
        }

        existingRelay.MarkAsDeleted();

        await relayRepository.DeleteAsync(request.RelayId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
