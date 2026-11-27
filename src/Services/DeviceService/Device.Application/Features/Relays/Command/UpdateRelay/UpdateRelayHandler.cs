using BuildingBlocks.Domain.Results;
using Device.Application.Interfaces;

namespace Device.Application.Features.Relays.Command.UpdateRelay;

internal sealed class UpdateRelayHandler(
    IRelayRepository relayRepository,
    IControllerRepository controllerRepository,
    IDeviceSecurityService securityService) : IRequestHandler<UpdateRelayCommand, Result>
{
    public async Task<Result> Handle(
        UpdateRelayCommand request,
        CancellationToken cancellationToken)
    {
        Relay? existingRelay = await relayRepository.GetByIdAsync(
            request.RelayId, cancellationToken);

        Guid previousControllerId = existingRelay!.ControllerId;

        if (request.ControllerId != previousControllerId)
        {
            Result newControllerOwnership = await securityService.EnsureUserOwnsControllerAsync(
                request.ControllerId, request.UserId, cancellationToken);
            if (newControllerOwnership.IsFailure)
            {
                return newControllerOwnership;
            }
        }

        Result result = existingRelay.Update(
            request.ControllerId, request.UserId,
            request.ConnectionProtocol, request.ConnectionAddress,
            request.Purpose, request.IsNormallyOpen);

        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        Controller? targetController = await controllerRepository.GetByIdAsync(request.ControllerId, cancellationToken);
        targetController?.IncrementConfigVersion("relay_updated");

        if (previousControllerId != request.ControllerId)
        {
            Controller? previousController = await controllerRepository.GetByIdAsync(previousControllerId, cancellationToken);
            previousController?.IncrementConfigVersion("relay_updated");
        }

        return Result.Success();
    }
}
