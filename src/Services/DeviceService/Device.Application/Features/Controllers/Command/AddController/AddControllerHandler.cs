using BuildingBlocks.Domain.Results;
using Device.Application.Interfaces;
using MassTransit;

namespace Device.Application.Features.Controllers.Command.AddController;

public sealed class AddControllerHandler(
    IDeviceTokenHasher tokenHasher,
    IControllerRepository controllerRepository) : IRequestHandler<AddControllerCommand, Result<ControllerRegisteredResponse>>
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

        return Result<ControllerRegisteredResponse>.Success(
            new ControllerRegisteredResponse
            {
                ControllerId = controller.Value.Id,
                DeviceToken = deviceToken
            });
    }
}
