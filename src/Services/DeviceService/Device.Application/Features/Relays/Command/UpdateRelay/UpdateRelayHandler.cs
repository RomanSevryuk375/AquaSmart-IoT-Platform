using Device.Application.Interfaces;

namespace Device.Application.Features.Relays.Command.UpdateRelay;

internal sealed class UpdateRelayHandler(
    IRelayRepository relayRepository,
    IDeviceSecurityService securityService,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateRelayCommand, Result>
{
    public async Task<Result> Handle(
        UpdateRelayCommand request,
        CancellationToken cancellationToken)
    {
        Relay? existingRelay = await relayRepository.GetByIdAsync(
            request.RelayId, cancellationToken);

        if (request.ControllerId != existingRelay!.ControllerId)
        {
            Result newControllerOwnership = await securityService.EnsureUserOwnsControllerAsync(
                request.ControllerId, cancellationToken);
            if (newControllerOwnership.IsFailure)
            {
                return newControllerOwnership;
            }
        }

        Result result = existingRelay.Update(
            request.ControllerId,
            request.ConnectionProtocol, request.ConnectionAddress,
            request.Purpose, request.IsNormallyOpen);
        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
