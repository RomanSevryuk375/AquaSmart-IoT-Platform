using BuildingBlocks.Domain.Results;
using Device.Application.Constants;
using Device.Application.Interfaces;
using MassTransit;
using ZiggyCreatures.Caching.Fusion;

namespace Device.Application.Features.Controllers.Command.AddController;

public sealed class AddControllerHandler(
    IDeviceTokenHasher tokenHasher,
    IControllerRepository controllerRepository,
    IFusionCache cache) : IRequestHandler<AddControllerCommand, Result<ControllerRegisteredResponse>>
{
    public async Task<Result<ControllerRegisteredResponse>> Handle(
        AddControllerCommand request,
        CancellationToken cancellationToken)
    {
        string deviceToken = tokenHasher.GenerateRawToken();
        string deviceTokenHash = tokenHasher.ComputeHash(deviceToken);

        Result<Controller> controller = Controller.Create(
            NewId.NextGuid(),
            request.UserId,
            request.MacAddress,
            deviceTokenHash,
            request.Name,
            request.IsOnline);
        if (controller.IsFailure)
        {
            return Result<ControllerRegisteredResponse>.Failure(
                controller.Error);
        }

        await controllerRepository.AddAsync(controller.Value, cancellationToken);
        await cache.RemoveAsync(
            CacheKeys.ControllerMqttAuth(controller.Value.MacAddress.Value),
            token: cancellationToken);

        return Result<ControllerRegisteredResponse>.Success(
            new ControllerRegisteredResponse
            {
                ControllerId = controller.Value.Id,
                DeviceToken = deviceToken
            });
    }
}
