namespace Device.Application.Features.Controllers.Command.ToggleCommandState;

internal sealed class ToggleControllerStateHandler(
    IControllerRepository controllerRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ToggleControllerStateCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(
        ToggleControllerStateCommand request,
        CancellationToken cancellationToken)
    {
        Controller? controller = await controllerRepository.GetByIdAsync(
            request.ControllerId, cancellationToken);
        if (controller is null)
        {
            return Result<bool>.Failure(Error.NotFound<Controller>(
                string.Format(ErrorMessages.ControllerNotFound, request.ControllerId)));
        }

        controller.ToggleState();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(controller.IsOnline);
    }
}
