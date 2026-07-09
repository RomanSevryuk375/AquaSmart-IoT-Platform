namespace Device.Application.Features.Controllers.Command.PingController;

internal sealed class PingControllerHandler(
    IControllerRepository controllerRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<PingControllerCommand, Result<ControllerPingResponse>>
{
    public async Task<Result<ControllerPingResponse>> Handle(
        PingControllerCommand request,
        CancellationToken cancellationToken)
    {
        Controller? controller = await controllerRepository.GetByIdAsync(
            request.ControllerId, cancellationToken);
        if (controller is null)
        {
            return Result<ControllerPingResponse>.Failure(Error.NotFound<Controller>(
                string.Format(ErrorMessages.ControllerNotFound, request.ControllerId)));
        }

        controller.RecordPing();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ControllerPingResponse>.Success(new ControllerPingResponse());
    }
}
