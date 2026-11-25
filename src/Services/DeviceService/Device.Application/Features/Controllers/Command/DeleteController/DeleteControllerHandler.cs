using BuildingBlocks.Domain.Results;
using Device.Application.Constants;
using ZiggyCreatures.Caching.Fusion;

namespace Device.Application.Features.Controllers.Command.DeleteController;

internal sealed class DeleteControllerHandler(
    IControllerRepository controllerRepository,
    IFusionCache cache)
    : IRequestHandler<DeleteControllerCommand, Result>
{
    public async Task<Result> Handle(
        DeleteControllerCommand request,
        CancellationToken cancellationToken)
    {
        Controller? controller = await controllerRepository.GetByIdAsync(request.ControllerId, cancellationToken);

        await controllerRepository.DeleteAsync(request.ControllerId, cancellationToken);

        await cache.RemoveAsync(CacheKeys.Controller(request.UserId, request.ControllerId), token: cancellationToken);
        await cache.RemoveAsync(CacheKeys.ControllerTokenAuth(request.ControllerId), token: cancellationToken);
        if (controller is not null)
        {
            await cache.RemoveAsync(CacheKeys.ControllerMqttAuth(controller.MacAddress.Value), token: cancellationToken);
        }

        return Result.Success();
    }
}
