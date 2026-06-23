using Device.Application.Interfaces;
using MassTransit;

namespace Device.Application.Features.Controllers.Command.AddController;

internal sealed class AddControllerHandler(
    IUserContext userContext,
    IMyHasher myHasher,
    IControllerRepository controllerRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<AddControllerCommand, Result<ControllerRegisteredResponse>>
{
    public async Task<Result<ControllerRegisteredResponse>> Handle(
        AddControllerCommand request,
        CancellationToken cancellationToken)
    {
        string deviceToken = NewId.NextGuid().ToString();

        Result<Controller> controller = Controller.Create(
            NewId.NextGuid(),
            userContext.UserId,
            request.MacAddress,
            myHasher.Generate(deviceToken),
            request.Name,
            request.IsOnline);
        if (controller.IsFailure)
        {
            return Result<ControllerRegisteredResponse>.Failure(
                controller.Error);
        }

        Guid result = await controllerRepository.AddAsync(controller.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ControllerRegisteredResponse>.Success(
            new ControllerRegisteredResponse
            {
                ControllerId = result,
                DeviceToken = deviceToken
            });
    }
}
