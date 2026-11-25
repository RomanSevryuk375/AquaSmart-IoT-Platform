using BuildingBlocks.Domain.Constants;
using BuildingBlocks.Domain.Results;
using Device.Application.Constants;
using ZiggyCreatures.Caching.Fusion;

namespace Device.Application.Features.Controllers.Command.UpdateController;

internal sealed class UpdateControllerHandler(
    IControllerRepository controllerRepository,
    IFusionCache cache)
    : IRequestHandler<UpdateControllerCommand, Result>
{
    public async Task<Result> Handle(
        UpdateControllerCommand request,
        CancellationToken cancellationToken)
    {
        Controller? controller = await controllerRepository.GetByIdAsync(
            request.ControllerId, cancellationToken);
        if (controller is null)
        {
            return Result<bool>.Failure(Error.NotFound<Controller>(
                string.Format(ErrorMessages.ControllerNotFound, request.ControllerId)));
        }

        string previousMac = controller.MacAddress.Value;
        Result? result = controller.Update(request.MacAddress, request.Name);
        if (result.IsSuccess)
        {
            await cache.RemoveAsync(CacheKeys.Controller(
                request.UserId, request.ControllerId), token: cancellationToken);
            await cache.RemoveAsync(CacheKeys.ControllerMqttAuth(previousMac), token: cancellationToken);

            string updatedMac = controller.MacAddress.Value;
            if (!string.Equals(previousMac, updatedMac, StringComparison.Ordinal))
            {
                await cache.RemoveAsync(CacheKeys.ControllerMqttAuth(updatedMac), token: cancellationToken);
            }
        }

        return result.IsFailure
            ? Result.Failure(result.Error)
            : Result.Success();
    }
}
