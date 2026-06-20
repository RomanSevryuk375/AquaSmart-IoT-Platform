using Device.Application.Interfaces;

namespace Device.Application.Features.Controllers.Command.PingController;

internal class PingControllerHandler(
    IControllerRepository controllerRepository,
    IDeviceSecurityService securityService,
    IUnitOfWork unitOfWork) : IRequestHandler<PingControllerCommand, Result<ControllerPingResponse>>
{
    public async Task<Result<ControllerPingResponse>> Handle(
        PingControllerCommand request,
        CancellationToken cancellationToken)
    {
        Controller? controller = await controllerRepository
            .GetByIdAsync(request.ControllerId, cancellationToken);

        Result ownership = await securityService.EnsureDeviceAccessAsync(
            request.ControllerId, request.DeviceToken, cancellationToken);
        if (ownership.IsFailure || controller is null)
        {
            return Result<ControllerPingResponse>
                .Failure(ownership.Error);
        }

        controller.RecordPing();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ControllerPingResponse>.Success(new ControllerPingResponse());
    }
}
